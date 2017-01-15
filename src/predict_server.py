'''
Predict Server
Create a server to accept image inputs and run them against a trained neural network.
This then sends the steering output back to the client.
Author: Tawn Kramer
'''
import argparse
import sys
import numpy as np
import h5py
import pygame
import json
from keras.models import model_from_json
#import matplotlib.pyplot as plt
import time
import asyncore
import json
import socket
from PIL import Image

class SteeringServer(asyncore.dispatcher):
    """Receives connections and establishes handlers for each client.
    """
    
    def __init__(self, address, model):
        asyncore.dispatcher.__init__(self)
        self.create_socket(socket.AF_INET, socket.SOCK_STREAM)
        self.bind(address)
        self.address = self.socket.getsockname()
        print 'binding to', self.address
        self.listen(1)
        self.model = model
        return

    def handle_accept(self):
        # Called when a client connects to our socket
        client_info = self.accept()
        #self.logger.debug('handle_accept() -> %s', client_info[1])
        print 'got a new client', client_info[1]
        h = SteeringHandler(sock=client_info[0], chunk_size=8*1024, model=self.model)
        return
    
    def handle_close(self):
        #self.logger.debug('handle_close()')
        self.close()
        return

class SteeringHandler(asyncore.dispatcher):
    """Handles echoing messages from a single client.
    """
    IDLE = 1
    GETTING_IMG = 2
    SENDING_STEERING = 3
    
    def __init__(self, sock, chunk_size=256, model=None):
        self.model = model
        self.chunk_size = chunk_size
        asyncore.dispatcher.__init__(self, sock=sock)
        self.data_to_write = []
        self.image_byes = []
        self.mode = self.IDLE
        return
    
    def writable(self):
        """We want to write if we have received data."""
        response = bool(self.data_to_write)
        return response
    
    def handle_write(self):
        """Write as much as possible of the most recent message we have received."""
        data = self.data_to_write.pop()
        sent = self.send(data[:self.chunk_size])
        if sent < len(data):
            remaining = data[sent:]
            self.data.to_write.append(remaining)
        elif self.mode == self.SENDING_STEERING:
          self.mode = self.IDLE

    def handle_read(self):
        """Read an incoming message from the client and put it into our outgoing queue."""
        data = self.recv(self.chunk_size)
        view_image = False
        #print 'got', len(data), 'bytes'
        if len(data) == 0:
          self.handle_close()
        elif self.mode == self.IDLE:
          try:
            jsonObj = json.loads(data)
            self.num_bytes = jsonObj['num_bytes']
            self.width = jsonObj['width']
            self.height = jsonObj['height']
            self.num_channels = jsonObj['num_channels']
            self.format = jsonObj['format']
            self.flip_y = jsonObj['flip_y']
            self.data_to_write.insert(0, "{ 'response' : 'ready_for_image' }")
            self.mode = self.GETTING_IMG
            self.image_byes = []
            self.num_read = 0
          except:
            self.mode = self.IDLE
            print 'failed to read json from: ', data
        elif self.mode == self.GETTING_IMG:
          self.image_byes.append(data)
          self.num_read += len(data)
          if self.num_read == self.num_bytes:
            lin_arr = np.fromstring(''.join(self.image_byes), dtype=np.uint8)
            self.mode = self.SENDING_STEERING
            if self.format == 'array_of_pixels':
              img = lin_arr.reshape(self.width, self.height, self.num_channels)
              if self.flip_y:
                img = np.flipud(img)
              img = img.transpose()
            else: #assumed to be ArrayOfChannels
              img = lin_arr.reshape(self.num_channels, self.width, self.height)
            if view_image:
              #this can be useful when validating that you have your images coming in correctly.
              vis_img = Image.fromarray(img.transpose(), 'RGB')
              vis_img.show()
              #this launches too many windows if we leave it up.
              self.handle_close()
            steering = self.model.predict(img[None, :, :, :])
            reply = '{ "steering" : "%f" }' % steering
            self.data_to_write.append(reply)
        else:
            print "wasn't prepared to recv request!"
    
    def handle_close(self):
        self.close()


# ***** main loop *****
if __name__ == "__main__":
  parser = argparse.ArgumentParser(description='prediction server')
  parser.add_argument('model', type=str, help='Path to model definition json. Model weights should be on the same path.')
  args = parser.parse_args()

  with open(args.model, 'r') as jfile:
    model = model_from_json(json.load(jfile))

  model.compile("sgd", "mse")
  weights_file = args.model.replace('json', 'keras')
  model.load_weights(weights_file)
  
  address = ('0.0.0.0', 9090)
  s = SteeringServer(address, model)
  asyncore.loop()



