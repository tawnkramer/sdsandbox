#!/usr/bin/env python
from __future__ import print_function
import argparse
import sys
import numpy as np
import h5py
import json
from keras.models import model_from_json
import matplotlib.pyplot as plt
import time
import os

# ***** main loop *****
if __name__ == "__main__":
  parser = argparse.ArgumentParser(description='Path viewer')
  parser.add_argument('model', type=str, help='model definition. Model weights should be on the same path.')
  parser.add_argument('dataset', type=str, help='Dataset/video clip name')
  args = parser.parse_args()
  model_file = os.path.join('..', 'outputs', 'steering_model', args.model) + ".json"

  with open(model_file, 'r') as jfile:
    model = model_from_json(json.load(jfile))

  model.compile("sgd", "mse")
  weights_file = model_file.replace('json', 'keras')
  model.load_weights(weights_file)

  # default dataset is the validation data on the highway
  dataset = args.dataset
  skip = 300

  log = h5py.File("../dataset/log/"+dataset+".h5", "r")
  cam = h5py.File("../dataset/camera/"+dataset+".h5", "r")

  pred_steer = []
  actual_steer = []
  start = time.time()
  iStart = 200
  iEnd = 700
  for iFrame in range(iStart, iEnd):
    img = cam['X'][iFrame] #first image for now.
    predicted_steers = model.predict(img[None, :, :, :])[0][0]
    #print 'predicted:', predicted_steers, 'actual:', log['steering_angle'][iFrame]
    pred_steer.append(predicted_steers)
    actual_steer.append(log['steering_angle'][iFrame])

  end = time.time()
  duration = end - start
  num_frames = iEnd - iStart
  print('it took', duration, 'sec to process %d frames.' % (num_frames))
  print('or an avg of', duration / num_frames, 'per frame' )
  plt.plot(pred_steer, color='r')
  plt.plot(actual_steer, color='b')
  plt.show()

