'''
Predict Server
Create a server to accept image inputs and run them against a trained neural network.
This then sends the steering output back to the client.
Author: Tawn Kramer
'''
from __future__ import print_function
import os
import sys
import argparse
import time
import json
import base64
import datetime
from io import BytesIO

import tensorflow as tf
from tensorflow.python import keras
from tensorflow.python.keras.models import load_model
from PIL import Image
import numpy as np
from gym_donkeycar.core.fps import FPSTimer
from gym_donkeycar.core.message import IMesgHandler
from gym_donkeycar.core.sim_client import SimClient

import conf
import models



if tf.__version__ == '1.13.1':
    from tensorflow import ConfigProto, Session

    # Override keras session to work around a bug in TF 1.13.1
    # Remove after we upgrade to TF 1.14 / TF 2.x.
    config = ConfigProto()
    config.gpu_options.allow_growth = True
    session = Session(config=config)
    keras.backend.set_session(session)



class DonkeySimMsgHandler(IMesgHandler):

    STEERING = 0
    THROTTLE = 1

    def __init__(self, model, constant_throttle, image_cb=None, rand_seed=0):
        self.model = model
        self.constant_throttle = constant_throttle
        self.client = None
        self.timer = FPSTimer()
        self.img_arr = None
        self.image_cb = image_cb
        self.steering_angle = 0.
        self.throttle = 0.
        self.rand_seed = rand_seed
        self.fns = {'telemetry' : self.on_telemetry,\
                    'car_loaded' : self.on_car_created,\
                    'on_disconnect' : self.on_disconnect,
                    'aborted' : self.on_aborted}

    def on_connect(self, client):
        self.client = client
        self.timer.reset()

    def on_aborted(self, msg):
        self.stop()

    def on_disconnect(self):
        pass

    def on_recv_message(self, message):
        self.timer.on_frame()
        if not 'msg_type' in message:
            print('expected msg_type field')
            print("message:", message)
            return

        msg_type = message['msg_type']
        if msg_type in self.fns:
            self.fns[msg_type](message)
        else:
            print('unknown message type', msg_type)

    def on_car_created(self, data):
        if self.rand_seed != 0:
            self.send_regen_road(0, self.rand_seed, 1.0)

    def on_telemetry(self, data):
        imgString = data["image"]
        image = Image.open(BytesIO(base64.b64decode(imgString)))
        img_arr = np.asarray(image, dtype=np.float32)
        self.img_arr = img_arr.reshape((1,) + img_arr.shape)

        if self.image_cb is not None:
            self.image_cb(img_arr, self.steering_angle )

    def update(self):
        if self.img_arr is not None:
            self.predict(self.img_arr)
            self.img_arr = None

    def predict(self, image_array):
        outputs = self.model.predict(image_array)
        self.parse_outputs(outputs)


    def parse_outputs(self, outputs):
        res = []

        # Expects the model with final Dense(2) with steering and throttle
        for i in range(outputs.shape[1]):
            res.append(outputs[0][i])

        self.on_parsed_outputs(res)
        
    def on_parsed_outputs(self, outputs):
        self.outputs = outputs
        self.steering_angle = 0.0
        self.throttle = 0.2

        if len(outputs) > 0:        
            self.steering_angle = outputs[self.STEERING]

        if self.constant_throttle != 0.0:
            self.throttle = self.constant_throttle
        elif len(outputs) > 1:
            self.throttle = outputs[self.THROTTLE] * conf.throttle_out_scale

        self.send_control(self.steering_angle, self.throttle)

    def send_control(self, steer, throttle):
        # print("send st:", steer, "th:", throttle)
        msg = { 'msg_type' : 'control', 'steering': steer.__str__(), 'throttle':throttle.__str__(), 'brake': '0.0' }
        self.client.queue_message(msg)

    def send_regen_road(self, road_style=0, rand_seed=0, turn_increment=0.0):
        '''
        Regenerate the road, where available. For now only in level 0.
        In level 0 there are currently 5 road styles. This changes the texture on the road
        and also the road width.
        The rand_seed can be used to get some determinism in road generation.
        The turn_increment defaults to 1.0 internally. Provide a non zero positive float
        to affect the curviness of the road. Smaller numbers will provide more shallow curves.
        '''
        msg = { 'msg_type' : 'regen_road',
            'road_style': road_style.__str__(),
            'rand_seed': rand_seed.__str__(),
            'turn_increment': turn_increment.__str__() }
        
        self.client.queue_message(msg)

    def stop(self):
        self.client.stop()

    def __del__(self):
        self.stop()



def clients_connected(arr):
    for client in arr:
        if not client.is_connected():
            return False
    return True


def go(filename, address, constant_throttle=0, num_cars=1, image_cb=None, rand_seed=None):

    print("loading model", filename)
    model = load_model(filename)

    # In this mode, looks like we have to compile it
    model.compile("sgd", "mse")

    clients = []

    for _ in range(0, num_cars):
        # setup the clients
        handler = DonkeySimMsgHandler(model, constant_throttle, image_cb=image_cb, rand_seed=rand_seed)
        client = SimClient(address, handler)
        clients.append(client)

    while clients_connected(clients):
        try:
            time.sleep(0.02)
            for client in clients:
                client.msg_handler.update()
        except KeyboardInterrupt:
            # unless some hits Ctrl+C and then we get this interrupt
            print('stopping')
            break


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='prediction server')
    parser.add_argument('--model', type=str, help='model filename')
    parser.add_argument('--host', type=str, default='127.0.0.1', help='server sim host')
    parser.add_argument('--port', type=int, default=9091, help='bind to port')
    parser.add_argument('--num_cars', type=int, default=1, help='how many cars to spawn')
    parser.add_argument('--constant_throttle', type=float, default=0.0, help='apply constant throttle')
    parser.add_argument('--rand_seed', type=int, default=0, help='set road generation random seed')
    args = parser.parse_args()

    address = (args.host, args.port)
    go(args.model, address, args.constant_throttle, num_cars=args.num_cars, rand_seed=args.rand_seed)
