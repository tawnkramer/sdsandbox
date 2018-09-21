'''
file: donkey_env.py
author: Tawn Kramer
date: 2018-08-31
'''
import os

import numpy as np
import gym
from gym import error, spaces, utils
from gym.utils import seeding
from donkey_gym.envs.donkey_sim import DonkeyUnitySimContoller
from donkey_gym.envs.donkey_proc import DonkeyUnityProcess

class DonkeyEnv(gym.Env):
    """
    OpenAI Gym Environment for Donkey
    """

    metadata = {
        "render.modes": ["human", "rgb_array"],
    }

    ACTION = ["steer", "throttle"]

    def __init__(self, level, time_step=0.05, frame_skip=2):

        # start Unity simulation subprocess
        self.proc = DonkeyUnityProcess()
        
        try:
            exe_path = os.environ['DONKEY_SIM_PATH']
        except:
            print("Missing DONKEY_SIM_PATH environment var. Using defaults")
            #you must start the executable on your own
            exe_path = "self_start"
        
        try:
            port = int(os.environ['DONKEY_SIM_PORT'])
        except:
            print("Missing DONKEY_SIM_PORT environment var. Using defaults")
            port = 9090
            
        try:
            headless = os.environ['DONKEY_SIM_HEADLESS']=='1'
        except:
            print("Missing DONKEY_SIM_HEADLESS environment var. Using defaults")
            headless = False

        self.proc.start(exe_path, headless=headless, port=port)

        # start simulation com
        self.viewer = DonkeyUnitySimContoller(level=level, time_step=time_step, port=port)

        # steering and throttle
        self.action_space = spaces.Discrete(len(self.ACTION))

        # camera sensor data
        self.observation_space = spaces.Box(0, 255, self.viewer.get_sensor_size())

        # simulation related variables.
        self.seed()

        # Frame Skipping
        self.frame_skip = frame_skip

        self.reset()

    def close(self):
        self.proc.quit()

    def seed(self, seed=None):
        self.np_random, seed = seeding.np_random(seed)
        return [seed]

    def step(self, action):
        for i in range(self.frame_skip):
            self.viewer.take_action(action)
            observation, reward, done, info = self.viewer.observe()
        return observation, reward, done, info

    def reset(self):
        self.viewer.reset()
        observation, reward, done, info = self.viewer.observe()
        return observation

    def render(self, mode="human", close=False):
        if close:
            self.viewer.quit()

        return self.viewer.render(mode)

    def is_game_over(self):
        return self.viewer.is_game_over()


## ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ##

class GeneratedRoadsEnv(DonkeyEnv):

    def __init__(self):
        super(GeneratedRoadsEnv, self).__init__(level=0)

class WarehouseEnv(DonkeyEnv):

    def __init__(self):
        super(WarehouseEnv, self).__init__(level=1)

class AvcSparkfunEnv(DonkeyEnv):

    def __init__(self):
        super(AvcSparkfunEnv, self).__init__(level=1)
