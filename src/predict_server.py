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
import throttle_manager
import shutil
import base64

import socketio
import eventlet
import eventlet.wsgi
from PIL import Image
from flask import Flask
from io import BytesIO
from datetime import datetime
import time

sio = socketio.Server()
app = Flask(__name__)
throttle_man = throttle_manager.ThrottleManager()
model = None
t = time.time()
iter = 0

@sio.on('telemetry')
def telemetry(sid, data):
    global iter
    global t
    iter += 1
    if iter == 100:
        e = time.time()
        print('fps', 100.0 / (e - t))
        t = time.time()
        iter = 0
    if data:
        # The current steering angle of the car
        steering_angle = float(data["steering_angle"])
        # The current throttle of the car
        throttle = float(data["throttle"])
        # The current speed of the car
        speed = float(data["speed"])
        # The current image from the center camera of the car
        imgString = data["image"]
        image = Image.open(BytesIO(base64.b64decode(imgString)))
        image_array = np.asarray(image)
        #lin_arr = np.fromstring(base64.b64decode(imgString), dtype=np.uint8)              
        #image_array = lin_arr.reshape(256, 256, 3)    
        if config.is_model_image_input_transposed(model):
              image_array = image_array.transpose()
    
        steering_angle = float(model.predict(image_array[None, :, :, :], batch_size=1))
        
        #set throttle value here
        throttle, brake = throttle_man.get_throttle_brake(speed, steering_angle)

        #print(steering_angle, throttle)
        send_control(steering_angle, throttle)

        # save frame
        if args.image_folder != '':
            timestamp = datetime.utcnow().strftime('%Y_%m_%d_%H_%M_%S_%f')[:-3]
            image_filename = os.path.join(args.image_folder, timestamp)
            #image.save('{}.jpg'.format(image_filename))
    else:
        # NOTE: DON'T EDIT THIS.
        sio.emit('manual', data={}, skip_sid=True)

@sio.on('connect')
def connect(sid, environ):
    print("connect ", sid)
    global t
    t = time.time()
    send_control(0, 0)

def send_control(steering_angle, throttle):
    sio.emit(
        "steer",
        data={
            'steering_angle': steering_angle.__str__(),
            'throttle': throttle.__str__()
        },
        skip_sid=True)

def go(model_json, address):
    global model
    global app

    #load keras model from the json file
    with open(model_json, 'r') as jfile:
        model = model_from_json(json.load(jfile))

    #In this mode, looks like we have to compile it
    model.compile("sgd", "mse")

    #load the weights file
    weights_file = model_json.replace('json', 'keras')
    model.load_weights(weights_file)

    # wrap Flask application with engineio's middleware
    app = socketio.Middleware(sio, app)

    # deploy as an eventlet WSGI server
    try:
        eventlet.wsgi.server(eventlet.listen(address), app)
    except KeyboardInterrupt:
        #unless some hits Ctrl+C and then we get this interrupt
        print('stopping')


# ***** main loop *****
if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='prediction server')
    parser.add_argument('model', type=str, help='model name. no json or keras.')
    parser.add_argument('--model-path', dest='path', default='../outputs/steering_model', help='model dir') 
    parser.add_argument(
          'image_folder',
          type=str,
          nargs='?',
          default='',
          help='Path to image folder. This is where the images from the run will be saved.'
      )

    args = parser.parse_args()

    if args.image_folder != '':
        print("Creating image folder at {}".format(args.image_folder))
        if not os.path.exists(args.image_folder):
            os.makedirs(args.image_folder)
        else:
            shutil.rmtree(args.image_folder)
            os.makedirs(args.image_folder)
        print("RECORDING THIS RUN ...")

    model_json = os.path.join(args.path, args.model +'.json')
    address = ('0.0.0.0', 9090)
    go(model_json, address)

