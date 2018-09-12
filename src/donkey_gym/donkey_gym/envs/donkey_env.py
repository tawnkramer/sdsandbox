'''
file: donkey_env.py
author: Tawn Kramer
date: 2018-08-31
'''
import os
from threading import Thread

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
        exe_path = os.environ['DONKEY_SIM_PATH']
        self.proc.start(exe_path, headless=False, platform="linux") #darwin for MacOS

        # start simulation com
        self.viewer = DonkeyUnitySimContoller(level, time_step)

        # steering and throttle
        self.action_space = spaces.Discrete(len(self.ACTION))

        # camera sensor data
        self.observation_space = spaces.Box(0, 255, self.viewer.get_sensor_size())

        # simulation related variables.
        self._seed()

        # Frame Skipping
        self.frame_skip = frame_skip

        #start donkey sim thread listener
        self.thread = Thread(target=self.viewer.listen)
        self.thread.start()

        self.reset()

    def seed(self, seed=None):
        self.np_random, seed = seeding.np_random(seed)
        return [seed]

    def step(self, action):
        for i in range(self.frame_skip):
            self.self.viewer.take_action(action)
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
