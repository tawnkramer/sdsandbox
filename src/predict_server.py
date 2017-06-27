#!/usr/bin/env python
'''
Predict Server
Create a server to accept image inputs and run them against a trained neural network.
This then sends the steering output back to the client.
Author: Tawn Kramer
'''
from __future__ import print_function
import os
import argparse
import sys
import numpy as np
import h5py
import json
from keras.models import model_from_json
import time
import asyncore
import json
import socket
from PIL import Image
import config

class SteeringServer(asyncore.dispatcher):
    """
      Receives network connections and establishes handlers for each client.
      Each client connection is handled by a new instance of the SteeringHandler class.
    """
    
    def __init__(self, address, model):
        asyncore.dispatcher.__init__(self)

        #create a TCP socket to listen for connections
        self.create_socket(socket.AF_INET, socket.SOCK_STREAM)

        #in case we have shutdown recently, allow the os to reuse this address. helps when restarting
        self.set_reuse_addr()

        #let TCP stack know that we'd like to sit on this address and listen for connections
        self.bind(address)
        
        #confirm for users what address we are listening on
        self.address = self.socket.getsockname()
        print('binding to', self.address)
        
        #let tcp stack know we plan to process one outstanding request to connect request each loop
        self.listen(1)

        #keep a pointer to our model to pass to the handler
        self.model = model

    def handle_accept(self):
        # Called when a client connects to our socket
        client_info = self.accept()
        
        print('got a new client', client_info[1])

        #make a new steering handler to communicate with the client
        SteeringHandler(sock=client_info[0], chunk_size=8*1024, model=self.model)
        
    
    def handle_close(self):
        # Called then server is shutdown
        self.close()

class SteeringHandler(asyncore.dispatcher):
    """
      Handles messages from a single TCP client.
      Uses a state machine given by the self.mode which goes
      through the IDLE, GETTING_IMAGES, and SENDING_STEERING states.
      When IDLE, we are expecting to get a message as a json object.
    """

    IDLE = 1
    GETTING_IMG = 2
    SENDING_STEERING = 3
    
    def __init__(self, sock, chunk_size=256, model=None):
        #we call our base class init
        asyncore.dispatcher.__init__(self, sock=sock)
        
        #model is the pointer to our Keras model we use for prediction
        self.model = model

        #chunk size is the max number of bytes to read per network packet
        self.chunk_size = chunk_size

        #we make an empty list of packets to send to the client here
        self.data_to_write = []

        #and image bytes is an empty list of partial bytes of the image as it comes in
        self.image_bytes = []

        #we start in an idle state
        self.mode = self.IDLE
    
    def writable(self):
        """
          We want to write if we have received data.
        """
        response = bool(self.data_to_write)

        return response
    
    def handle_write(self):
        """
          Write as much as possible of the most recent message we have received.
          This is only called by async manager when the socket is in a writable state
          and when self.writable return true, that yes, we have data to send.
        """

        #pop the first element from the list. encode will make it into a byte stream
        data = self.data_to_write.pop(0).encode()

        #send a slice of that data, up to a max of the chunk_size
        sent = self.send(data[:self.chunk_size])
        
        #if we didn't send all the data..
        if sent < len(data):
            #then slick off the portion that remains to be sent
            remaining = data[sent:]

            #since we've popped it off the list, add it back to the list to send next
            #probably should change this to a deque...
            self.data.to_write.insert(0, remaining)

        elif self.mode == self.SENDING_STEERING:
          #if we have just sent the steering data, then we are idle again waiting for the next
          #image header information
          self.mode = self.IDLE


    def handle_read(self):
        """
          Read an incoming message from the client and put it into our outgoing queue.
          handle_read should only be called when the given socket has data ready to be
          processed.
        """

        #receive a chunK of data with the max size chunk_size from our client.
        data = self.recv(self.chunk_size)
        
        if len(data) == 0:
          #this only happens when the connection is dropped
          self.handle_close()
          print('connection dropped')

        elif self.mode == self.IDLE:
          '''
          We are expecing a json object from the client telling us what kind of image they are sending.
          '''
          try:
            #convert data into a string with decode, and then load it as a json object
            jsonObj = json.loads(data.decode("utf-8"))

            #num_bytes should be the total size of the image that will be sent next.
            #this is commonly the same size each time
            self.num_bytes = jsonObj['num_bytes']

            #this is the width, height, and channel depth of the pixels of the image
            self.width = jsonObj['width']
            self.height = jsonObj['height']
            self.num_channels = jsonObj['num_channels']

            #some images come in a format that needs to be flipped in the y direction
            self.flip_y = jsonObj['flip_y']

            #add this json object to the array of data to send back to the client
            self.data_to_write.insert(0, "{ 'response' : 'ready_for_image' }")

            #and change our mode to let us know the next packet should be image data
            self.mode = self.GETTING_IMG

            #and reset our container which will hold image data as it comes in            
            self.image_bytes = []
            self.num_read = 0
        
          except:
            #something bad happened, usually malformed json packet. jump back to idle and hope things continue
            self.mode = self.IDLE
            print('failed to read json from: ', data)
        
        elif self.mode == self.GETTING_IMG:
          
          #append each packet we've recieved into the image_bytes list
          self.image_bytes.append(data)
          self.num_read += len(data)
          
          #when we've read exactly as many bytes as was intented for this image..
          if self.num_read == self.num_bytes:
            #then try unpacking it into a numpy array, first by
            #joining the array of image bytes, as python3 does
            #and then as an array of string data, as python2 does..
            try:
              lin_arr = np.frombuffer(b''.join(self.image_bytes), dtype=np.uint8)              
            except:
              lin_arr = np.fromstring(''.join(self.image_bytes), dtype=np.uint8)
            
            #reshape the array into the image dimensions to make model.predict happy
            img = lin_arr.reshape(self.width, self.height, self.num_channels)
        
            #flip images in y, if they need it
            if self.flip_y:
              img = np.flipud(img)
        
            #tranpose images if we've been configured to do so.
            if config.is_model_image_input_transposed(self.model):
              img = img.transpose()
            
            #call model.predict with our image and capture the output in the steering variable
            steering = self.model.predict(img[None, :, :, :])
            
            #our json object with steering information
            reply = '{ "steering" : "%f" }' % steering
            
            #once we have image data, we are going to do our prediction and then send the
            #steering information
            self.mode = self.SENDING_STEERING

            #queue the packet to send by adding it to our list of data_to_write
            self.data_to_write.append(reply)
        
          elif self.num_read > self.num_bytes:
            #oops, did we get too many bytes? Don't stall, just flip back to idle mode and hope for the best.            
            print('problem, read too many bytes!')
            self.mode = self.IDLE
        
        else:
            print("wasn't prepared to recv request!")
    
    def handle_close(self):
        #when client drops or closes connection
        self.close()

def go(model_json, address):

  #load keras model from the json file
  with open(model_json, 'r') as jfile:
      model = model_from_json(json.load(jfile))

  #In this mode, looks like we have to compile it
  model.compile("sgd", "mse")

  #load the weights file
  weights_file = model_json.replace('json', 'keras')
  model.load_weights(weights_file)
  
  #setup the steering server to serve predictions
  s = SteeringServer(address, model)

  try:
    #asyncore.loop() will keep looping as long as any asyncore dispatchers are alive
    asyncore.loop()
  except KeyboardInterrupt:
    #unless some hits Ctrl+C and then we get this interrupt
    print('stopping')

# ***** main loop *****
if __name__ == "__main__":
  parser = argparse.ArgumentParser(description='prediction server')
  parser.add_argument('model', type=str, help='model name. no json or keras.')
  parser.add_argument('--model-path', dest='path', default='../outputs/steering_model', help='model dir') 
  args = parser.parse_args()

  model_json = os.path.join(args.path, args.model +'.json')
  address = ('0.0.0.0', 9090)
  go(model_json, address)

