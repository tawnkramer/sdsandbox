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
from keras.models import load_model
#import matplotlib.pyplot as plt
import time
import asyncore
import json
import socket
from PIL import Image
import pygame
import conf
import predict_server

pygame.init()
ch, row, col = conf.ch, conf.row, conf.col

size = (col*2, row*2)
pygame.display.set_caption("sdsandbox data monitor")
screen = pygame.display.set_mode(size, pygame.DOUBLEBUF)
camera_surface = pygame.surface.Surface((col,row),0,24).convert()
myfont = pygame.font.SysFont("monospace", 15)

def screen_print(x, y, msg, screen):
    label = myfont.render(msg, 1, (255,255,0))
    screen.blit(label, (x, y))

def display_img(img, steering):
    img = img.swapaxes(0, 1)
    # draw frame
    pygame.surfarray.blit_array(camera_surface, img)
    camera_surface_2x = pygame.transform.scale2x(camera_surface)
    screen.blit(camera_surface_2x, (0,0))
    #steering value
    screen_print(10, 10, 'NN    :' + str(steering), screen)
    pygame.display.flip()

# ***** main loop *****
if __name__ == "__main__":
  parser = argparse.ArgumentParser(description='prediction server with monitor')
  parser.add_argument('model', type=str, help='model name. no json or keras.')
  args = parser.parse_args()
 
  address = ('0.0.0.0', 9090)
  
  try:
    predict_server.run_steering_server(address, args.model, image_folder=None, image_cb=display_img)   
  except KeyboardInterrupt:
    print('got ctrl+c break')

  

