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
import json
from keras.models import load_model
import time
import asyncore
import json
import socket
from PIL import Image
from io import BytesIO
import base64
import datetime

from donkey_gym.core.fps import FPSTimer
from donkey_gym.core.tcp_server import IMesgHandler, SimServer
from donkeycar.contrib.coordconv.coord import CoordinateChannel2D
from donkeycar.utils import linear_unbin


class DonkeySimMsgHandler(IMesgHandler):

    STEERING = 0
    THROTTLE = 1

    def __init__(self, model, iSceneToLoad=3):
        self.iSceneToLoad = iSceneToLoad
        self.model = model
        self.sock = None
        self.timer = FPSTimer()
        self.image_folder = None
        self.fns = {'telemetry' : self.on_telemetry,
                    "scene_selection_ready" : self.on_scene_selection_ready,
                    "scene_names": self.on_recv_scene_names }

    def on_connect(self, socketHandler):
        self.sock = socketHandler
        self.timer.reset()

    def on_recv_message(self, message):
        self.timer.on_frame()
        if not 'msg_type' in message:
            print('expected msg_type field')
            return

        msg_type = message['msg_type']
        if msg_type in self.fns:
            self.fns[msg_type](message)
        else:
            print('unknown message type', msg_type)

    def on_telemetry(self, data):
        imgString = data["image"]
        image = Image.open(BytesIO(base64.b64decode(imgString)))
        image_array = np.asarray(image)

        #name of object we just hit. "none" if nothing.
        hit = data["hit"]

        if hit != "none":
            self.send_reset_car()
        else:
            self.predict(image_array)

        # maybe save frame
        if self.image_folder is not None:
            timestamp = datetime.utcnow().strftime('%Y_%m_%d_%H_%M_%S_%f')[:-3]
            image_filename = os.path.join(self.image_folder, timestamp)
            image.save('{}.jpg'.format(image_filename))


    def predict(self, image_array):
        outputs = self.model.predict(image_array[None, :, :, :])
        self.parse_outputs(outputs)
    
    def parse_outputs(self, outputs):
        res = []
        for iO, output in enumerate(outputs):
            if len(output.shape) == 2:
                if iO == self.STEERING:
                    steering_angle = linear_unbin(output)
                    res.append(steering_angle)
                elif iO == self.THROTTLE:
                    throttle = linear_unbin(output, N=output.shape[1], offset=0.0, R=0.5)
                    res.append(throttle)
                else:
                    res.append( np.argmax(output) )
            else:
                res.append(output[0])

        self.on_parsed_outputs(res)
        
    def on_parsed_outputs(self, outputs):
        self.outputs = outputs
        steering_angle = outputs[self.STEERING]
        throttle = outputs[self.THROTTLE]
        self.send_control(steering_angle, throttle)

    def on_scene_selection_ready(self, data):
        print("SceneSelectionReady ")
        self.send_get_scene_names()

    def on_recv_scene_names(self, data):
        if data:
            names = data['scene_names']
            print("SceneNames:", names)
            self.send_load_scene(names[self.iSceneToLoad])

    def send_control(self, steer, throttle):
        msg = { 'msg_type' : 'control', 'steering': steer.__str__(), 'throttle':throttle.__str__(), 'brake': '0.0' }
        self.sock.queue_message(msg)
        
    def send_reset_car(self):
        msg = { 'msg_type' : 'reset_car' }
        self.sock.queue_message(msg)

    def send_get_scene_names(self):
        msg = { 'msg_type' : 'get_scene_names' }
        self.sock.queue_message(msg)

    def send_load_scene(self, scene_name):
        msg = { 'msg_type' : 'load_scene', 'scene_name' : scene_name }
        self.sock.queue_message(msg)


    def on_close(self):
        pass



def go(filename, address):

    model = load_model(filename)

    #In this mode, looks like we have to compile it
    model.compile("sgd", "mse")
  
    #setup the server
    handler = DonkeySimMsgHandler(model)
    server = SimServer(address, handler)

    try:
        #asyncore.loop() will keep looping as long as any asyncore dispatchers are alive
        asyncore.loop()
    except KeyboardInterrupt:
        #unless some hits Ctrl+C and then we get this interrupt
        print('stopping')

# ***** main loop *****
if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='prediction server')
    parser.add_argument('--model', type=str, help='model filename')
    args = parser.parse_args()

    address = ('0.0.0.0', 9091)
    go(args.model, address)
