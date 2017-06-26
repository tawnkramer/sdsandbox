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
#import matplotlib.pyplot as plt
import time
import asyncore
import json
import socket
from PIL import Image
import config

class SteeringServer(asyncore.dispatcher):
    """Receives connections and establishes handlers for each client.
    """
    
    def __init__(self, address, model):
        asyncore.dispatcher.__init__(self)
        self.create_socket(socket.AF_INET, socket.SOCK_STREAM)
        self.set_reuse_addr()
        self.bind(address)
        self.address = self.socket.getsockname()
        print('binding to', self.address)
        self.listen(1)
        self.model = model
        return

    def handle_accept(self):
        # Called when a client connects to our socket
        client_info = self.accept()
        #self.logger.debug('handle_accept() -> %s', client_info[1])
        print('got a new client', client_info[1])
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
        data = self.data_to_write.pop().encode()
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
            jsonObj = json.loads(data.decode("utf-8"))
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
            print('failed to read json from: ', data)
        elif self.mode == self.GETTING_IMG:
          self.image_byes.append(data)
          self.num_read += len(data)
          if self.num_read == self.num_bytes:
            try:
              lin_arr = np.frombuffer(b''.join(self.image_byes), dtype=np.uint8)              
            except:
              lin_arr = np.fromstring(''.join(self.image_byes), dtype=np.uint8)
            self.mode = self.SENDING_STEERING
            if self.format == 'array_of_pixels':
              img = lin_arr.reshape(self.width, self.height, self.num_channels)
              if self.flip_y:
                img = np.flipud(img)
              if config.image_tranposed:
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
          elif self.num_read > self.num_bytes:
            print('problem, read too many bytes!')
            self.mode = self.IDLE
        else:
            print("wasn't prepared to recv request!")
    
    def handle_close(self):
        self.close()

def go(model_json, address):
  with open(model_json, 'r') as jfile:
      model = model_from_json(json.load(jfile))

  model.compile("sgd", "mse")
  weights_file = model_json.replace('json', 'keras')
  model.load_weights(weights_file)
  
  s = SteeringServer(address, model)
  try:
    asyncore.loop()
  except KeyboardInterrupt:
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

