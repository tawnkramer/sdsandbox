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
from tensorflow.keras.models import load_model
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
import conf

class DonkeySimMsgHandler(IMesgHandler):

    STEERING = 0
    THROTTLE = 1

    def __init__(self, model, constant_throttle, port=0, num_cars=1, image_cb=None):
        self.model = model
        self.constant_throttle = constant_throttle
        self.sock = None
        self.timer = FPSTimer()
        self.image_folder = None
        self.image_cb = image_cb
        self.steering_angle = 0.
        self.throttle = 0.
        self.num_cars = 0
        self.port = port
        self.target_num_cars = num_cars
        self.fns = {'telemetry' : self.on_telemetry,\
                    'car_loaded' : self.on_car_created,\
                    'on_disconnect' : self.on_disconnect}

    def on_connect(self, socketHandler):
        self.sock = socketHandler
        self.timer.reset()

    def on_disconnect(self):
        self.num_cars = 0

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

    def on_car_created(self, data):
        self.num_cars += 1
        if self.num_cars < self.target_num_cars:
            print("requesting another car..")
            self.request_another_car()

    def on_telemetry(self, data):
        imgString = data["image"]
        image = Image.open(BytesIO(base64.b64decode(imgString)))
        image_array = np.asarray(image)
        self.predict(image_array)

        if self.image_cb is not None:
            self.image_cb(image_array, self.steering_angle )

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
                    self.steering_angle = linear_unbin(output)
                    res.append(self.steering_angle)
                elif iO == self.THROTTLE:
                    self.throttle = linear_unbin(output, N=output.shape[1], offset=0.0, R=0.5)
                    res.append(self.throttle)
                else:
                    res.append( np.argmax(output) )
            else:
                for i in range(output.shape[0]):
                    res.append(output[i])

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
        msg = { 'msg_type' : 'control', 'steering': steer.__str__(), 'throttle':throttle.__str__(), 'brake': '0.0' }
        #print(steer, throttle)
        self.sock.queue_message(msg)

    def request_another_car(self):
        port = self.port + self.num_cars
        address = ("0.0.0.0", port)
        
        #spawn a new message handler serving on the new port.
        handler = DonkeySimMsgHandler(self.model, 0., num_cars=(self.target_num_cars - 1), port=address[1])
        server = SimServer(address, handler)

        msg = { 'msg_type' : 'new_car', 'host': '127.0.0.1', 'port' : port.__str__() }
        self.sock.queue_message(msg)   

    def on_close(self):
        pass



def go(filename, address, constant_throttle=0, num_cars=1, image_cb=None):

    model = load_model(filename)

    #In this mode, looks like we have to compile it
    model.compile("sgd", "mse")
  
    #setup the server
    handler = DonkeySimMsgHandler(model, constant_throttle, port=address[1], num_cars=num_cars, image_cb=image_cb)
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
    parser.add_argument('--host', type=str, default='0.0.0.0', help='bind to ip')
    parser.add_argument('--port', type=int, default=9091, help='bind to port')
    parser.add_argument('--num_cars', type=int, default=1, help='how many cars to spawn')
    parser.add_argument('--constant_throttle', type=float, default=0.0, help='apply constant throttle')
    args = parser.parse_args()

    address = (args.host, args.port)
    go(args.model, address, args.constant_throttle, num_cars=args.num_cars)
