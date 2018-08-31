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
import json
import time
from datetime import datetime
import asyncore
import json
import shutil
import base64
import random

import numpy as np
import h5py
from PIL import Image
import socketio
import eventlet
import eventlet.wsgi
from PIL import Image
from flask import Flask
from io import BytesIO
import keras

import conf
import throttle_manager


sio = socketio.Server()
app = Flask(__name__)
throttle_man = throttle_manager.ThrottleManager()
model = None
iSceneToLoad = 0
time_step = 0.1
step_mode = "synchronous"

class FPSTimer(object):
    def __init__(self):
        self.t = time.time()
        self.iter = 0

    def reset(self):
        self.t = time.time()
        self.iter = 0

    def on_frame(self):
        self.iter += 1
        if self.iter == 100:
            e = time.time()
            print('fps', 100.0 / (e - self.t))
            self.t = time.time()
            self.iter = 0

timer = FPSTimer()

@sio.on('Telemetry')
def telemetry(sid, data):
    global timer
    global num_frames_to_send
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

        #name of object we just hit. "none" if nothing.
        hit = data["hit"]

        x = data["pos_x"]
        y = data["pos_y"]
        z = data["pos_z"]

        #Cross track error not always present.
        #Will be missing if path is not setup in the given scene.
        #It should be setup in the 3 scenes available now.
        try:
            cte = data["cte"]
        except:
            pass

        #print("x", x, "y", y, "cte", cte, "hit", hit)

        outputs = model.predict(image_array[None, :, :, :])

        #steering
        steering_angle = outputs[0][0]

        #do we get throttle from our network?
        if conf.num_outputs == 2 and len(outputs[0]) == 2:
            throttle = outputs[0][1]
        else:
            #set throttle value here
            throttle, brake = throttle_man.get_throttle_brake(speed, steering_angle)
        
        #print(steering_angle, throttle)

        #reset scene to start if we hit anything.
        if hit != "none":
            send_exit_scene()
        else:
            send_control(steering_angle, throttle)

        # save frame
        if args.image_folder != '':
            timestamp = datetime.utcnow().strftime('%Y_%m_%d_%H_%M_%S_%f')[:-3]
            image_filename = os.path.join(args.image_folder, timestamp)
            #image.save('{}.jpg'.format(image_filename))
    else:
        # NOTE: DON'T EDIT THIS.
        sio.emit('RequestTelemetry', data={}, skip_sid=True)

    timer.on_frame()

@sio.on('connect')
def connect(sid, environ):
    print("connect ", sid)
    global timer
    global time_step
    global step_mode
    timer.reset()

    send_settings({"step_mode" : step_mode.__str__(),\
         "time_step" : time_step.__str__()})
    
    send_control(0, 0)

@sio.on('ProtocolVersion')
def on_proto_version(sid, environ):
    print("ProtocolVersion ", sid)

@sio.on('SceneSelectionReady')
def on_fe_loaded(sid, environ):
    print("SceneSelectionReady ", sid)
    send_get_scene_names()

@sio.on('SceneLoaded')
def on_scene_loaded(sid, data):
    print("SceneLoaded ", sid)

@sio.on('SceneNames')
def on_scene_names(sid, data):
    print("SceneNames ", sid)
    if data:
        names = data['scene_names']
        print("SceneNames:", names)
        global iSceneToLoad
        send_load_scene(names[iSceneToLoad])

def send_get_scene_names():
    sio.emit(
        "GetSceneNames",
        data={            
        },
        skip_sid=True)

def send_control(steering_angle, throttle):
    sio.emit(
        "Steer",
        data={
            'steering_angle': steering_angle.__str__(),
            'throttle': throttle.__str__()
        },
        skip_sid=True)

def send_load_scene(scene_name):
    print("Loading", scene_name)
    sio.emit(
        "LoadScene",
        data={
            'scene_name': scene_name.__str__()
        },
        skip_sid=True)

def send_exit_scene():
    sio.emit(
        "ExitScene",
        data={
            'none': 'none'
        },
        skip_sid=True)

def send_reset_car():
    sio.emit(
        "ResetCar",
        data={            
        },
        skip_sid=True)

def send_settings(prefs):
    sio.emit(
        "Settings",
        data=prefs,
        skip_sid=True)

def go(model_fnm, address, iScene):
    global model
    global app
    global iSceneToLoad

    model = keras.models.load_model(model_fnm)

    #In this mode, looks like we have to compile it
    model.compile("sgd", "mse")

    # wrap Flask application with engineio's middleware
    app = socketio.Middleware(sio, app)

    #which scene to load
    iSceneToLoad = iScene

    # deploy as an eventlet WSGI server
    try:
        eventlet.wsgi.server(eventlet.listen(address), app)
    except KeyboardInterrupt:
        #unless some hits Ctrl+C and then we get this interrupt
        print('stopping')


# ***** main loop *****
if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='sim_server')
    parser.add_argument('model', type=str, help='model name')
    parser.add_argument('--i_scene', default=0, help='which scene to load')
    parser.add_argument('--step_mode', default="asynchronous", help='how to advance time in sim (asynchronous|synchronous)')
    parser.add_argument('--time_step', type=float, default=0.1, help='how far to advance time in sim when synchronous')
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

    time_step = args.time_step
    step_mode = args.step_mode
    iScene = int(args.i_scene)
    model_fnm = args.model
    address = ('0.0.0.0', 9090)
    go(model_fnm, address, iScene)

