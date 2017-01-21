#!/usr/bin/env python
import argparse
import sys
import numpy as np
import h5py
import pygame
import json
from keras.models import model_from_json
import pdb
import camera_format

pygame.init()
ch, row, col = camera_format.get_camera_image_dim()

size = (col*2, row*2)
pygame.display.set_caption("sdsandbox data viewer")
screen = pygame.display.set_mode(size, pygame.DOUBLEBUF)
camera_surface = pygame.surface.Surface((col,row),0,24).convert()
myfont = pygame.font.SysFont("monospace", 15)

def screen_print(x, y, msg, screen):
  label = myfont.render(msg, 1, (255,255,0))
  screen.blit(label, (x, y))

# ***** main loop *****
if __name__ == "__main__":
  parser = argparse.ArgumentParser(description='Path viewer')
  parser.add_argument('model', type=str, help='Path to model definition json. Model weights should be on the same path.')
  parser.add_argument('--dataset', type=str, default="tawn_Tue_Dec_27_19_59_07_2016", help='Dataset/video clip name')
  args = parser.parse_args()

  with open(args.model, 'r') as jfile:
    model = model_from_json(json.load(jfile))

  model.compile("sgd", "mse")
  weights_file = args.model.replace('json', 'keras')
  model.load_weights(weights_file)

  # default dataset is the validation data on the highway
  dataset = args.dataset
  
  log = h5py.File("../dataset/log/"+dataset+".h5", "r")
  cam = h5py.File("../dataset/camera/"+dataset+".h5", "r")

  print log.keys()

  iStart = 0
  iEnd = len(cam['X'])
  iFrame = 0
  play = True

  while True:
    
    keys = pygame.key.get_pressed()

    if keys[pygame.K_SPACE]:
      play = play

    if keys[pygame.K_RIGHT]:
      if iFrame < iEnd - 1:
        iFrame += 1

    if keys[pygame.K_LEFT]:
      if iFrame > 0:
        iFrame += -1
        
    if play and iFrame < iEnd - 1:
      iFrame += 1
    
    img = cam['X'][iFrame]
    steering = model.predict(img[None, :, :, :])[0][0]
    
    angle_steers = log['steering_angle'][iFrame]
    speed_ms = log['speed'][iFrame]
    img = img.transpose().swapaxes(0, 1)

    # draw frame
    pygame.surfarray.blit_array(camera_surface, img)
    camera_surface_2x = pygame.transform.scale2x(camera_surface)
    screen.blit(camera_surface_2x, (0,0))
    #steering value
    screen_print(10, 10, str(steering), screen)
    pygame.display.flip()
