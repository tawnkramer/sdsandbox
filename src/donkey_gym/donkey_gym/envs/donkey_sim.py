'''
file: donkey_sim.py
author: Tawn Kramer
date: 2018-08-31
'''

import json
import shutil
import base64
import random
import time
from io import BytesIO
import math
from threading import Thread

import numpy as np
import socketio
import eventlet
import eventlet.wsgi
from PIL import Image
from flask import Flask

sio = socketio.Server()


# This is to monkey_patch python standard library "magically". Without it server cannot actively push messages to Unity client through emit()
# Reference: https://github.com/miguelgrinberg/Flask-SocketIO/issues/357
#Not on a mac
#eventlet.monkey_patch()

class DonkeyUnitySimContoller(object):

    #cross track error max
    CTE_MAX_ERR = 5.0

    def __init__(self, level, time_step=0.05, port=9090):
        self.level = level
        self.time_step = time_step
        self.verbose = False
        self.wait_time_for_obs = 0.01

        # sensor size - height, width, depth
        self.camera_img_size=(120, 160, 3)

        self.address = ('0.0.0.0', port)        
        
        self.app = Flask("DonkeyUnitySim")

        self.reset(intial=True)

        #start donkey sim listener
        self.thread = Thread(target=self.listen)
        self.thread.daemon = True
        self.thread.start()

    ## ------- Env interface ---------- ##

    def reset(self, intial=False):
        if self.verbose:
            print("reseting")
        self.image_array = np.zeros(self.camera_img_size)
        self.hit = "none"
        self.cte = 0.0
        self.x = 0.0
        self.y = 0.0
        self.z = 0.0
        self.loaded = False
        self.wait_for_obs = True
        self.have_new_obs = False
        self.show_obs_state()
        if not intial:
            #this exit scene command will cause us to re-load the scene.
            #on scene load we will have a fresh observation to return from observe
            #self.send_exit_scene()
            #The exit scene will work, but it's slow to disconnect and re-connect.
            #I think we have vehicle physics resetting properly now, and it's much faster.
            #So try that:
            self.send_reset_car()
        if self.verbose:
            print("RequestTelemetry")
        sio.emit('RequestTelemetry', data={}, skip_sid=True)
    
    def show_obs_state(self):
        if self.verbose:
            print("have_new_obs", self.have_new_obs)

    def get_sensor_size(self):
        return self.camera_img_size

    def take_action(self, action):
        if self.verbose:
            print("take_action")
        self.wait_for_obs = True
        self.have_new_obs = False
        self.show_obs_state()
        self.send_control(action[0], action[1])        

    def observe(self):
        assert(self.wait_for_obs)
        while not self.have_new_obs:
            self.show_obs_state()
            time.sleep(self.wait_time_for_obs)
            if self.verbose:
                print("waiting on new obs")
                print("RequestTelemetry")
            sio.emit('RequestTelemetry', data={}, skip_sid=True)

        observation = self.image_array
        done = self.is_game_over()
        reward = self.calc_reward(done)
        info = {}

        if self.verbose:
            print("observe ready for new obs")
        self.wait_for_obs = False
        self.have_new_obs = False
        self.show_obs_state()

        return observation, reward, done, info

    def quit(self):
        pass

    def render(self, mode):
        pass

    def is_game_over(self):
        return self.hit != "none" or math.fabs(self.cte) > self.CTE_MAX_ERR

    ## ------ RL interface ----------- ##

    def calc_reward(self, done):
        if done:
            return -1.0

        if self.cte > self.CTE_MAX_ERR:
            return -1.0

        return 1.0 - (self.cte / self.CTE_MAX_ERR)


    ## ------ Websocket interface ----------- ##

    def listen(self):

        @sio.on('Telemetry')
        def telemetry(sid, data):
            if self.verbose:
                print("got telemetry", data is not None)

            if data:
                # The current image from the center camera of the car
                imgString = data["image"]
                image = Image.open(BytesIO(base64.b64decode(imgString)))
                self.image_array = np.asarray(image)

                #name of object we just hit. "none" if nothing.
                self.hit = data["hit"]

                self.x = data["pos_x"]
                self.y = data["pos_y"]
                self.z = data["pos_z"]

                #Cross track error not always present.
                #Will be missing if path is not setup in the given scene.
                #It should be setup in the 3 scenes available now.
                try:
                    self.cte = data["cte"]
                except:
                    pass

                self.have_new_obs = True
                self.show_obs_state()

            else:
                if self.verbose:
                    print("RequestTelemetry")
                sio.emit('RequestTelemetry', data={}, skip_sid=True)

        @sio.on('connect')
        def connect(sid, environ):
            print("web socket connected")
            step_mode = "synchronous"

            self.send_settings({"step_mode" : step_mode.__str__(),\
                "time_step" : self.time_step.__str__()})

        @sio.on('ProtocolVersion')
        def on_proto_version(sid, environ):
            if self.verbose:
                print("got ProtocolVersion", environ)

        @sio.on('SceneSelectionReady')
        def on_fe_loaded(sid, environ):
            if self.verbose:
                print("got SceneSelectionReady", environ)
            self.send_get_scene_names()
            

        @sio.on('SceneLoaded')
        def on_scene_loaded(sid, data):
            if self.verbose:
                print("got SceneLoaded", data)
            self.take_action((0, 0))


        @sio.on('SceneNames')
        def on_scene_names(sid, data):
            if self.verbose:
                print("got SceneNames", data)
            names = data['scene_names']
            self.send_load_scene(names[self.level])

        # wrap Flask application with engineio's middleware
        self.app = socketio.Middleware(sio, self.app)

        # deploy as an eventlet WSGI server
        try:
            print("listening on", self.address)
            eventlet.wsgi.server(eventlet.listen(self.address), self.app)
        except KeyboardInterrupt:
            #unless some hits Ctrl+C and then we get this interrupt
            print('stopping')



    ## ------- Unity Event Messages --------------- ##

    def send_get_scene_names(self):
        if self.verbose:
            print("send_get_scene_names")
        sio.emit(
            "GetSceneNames",
            data={            
            },
            skip_sid=True)

    def send_control(self, steering_angle, throttle):
        if self.verbose:
            print("send_control")
        sio.emit(
            "Steer",
            data={
                'steering_angle': steering_angle.__str__(),
                'throttle': throttle.__str__()
            },
            skip_sid=True)

    def send_load_scene(self, scene_name):
        print("Loading", scene_name)
        sio.emit(
            "LoadScene",
            data={
                'scene_name': scene_name.__str__()
            },
            skip_sid=True)

    def send_exit_scene(self):
        if self.verbose:
            print("send_exit_scene")
        sio.emit(
            "ExitScene",
            data={
                'none': 'none'
            },
            skip_sid=True)

    def send_reset_car(self):
        if self.verbose:
            print("send_reset_car")
        sio.emit(
            "ResetCar",
            data={            
            },
            skip_sid=True)

    def send_settings(self, prefs):
        if self.verbose:
            print("send_settings")
        sio.emit(
            "Settings",
            data=prefs,
            skip_sid=True)


