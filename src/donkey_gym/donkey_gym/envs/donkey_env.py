'''
file: donkey_env.py
author: Tawn Kramer
date: 2018-08-31
'''
from threading import Thread

import numpy as np
import gym
from gym import error, spaces, utils
from gym.utils import seeding
from donkey_sim import DonkeyUnitySim

class DonkeyEnv(gym.Env):
    metadata = {
        "render.modes": ["human", "rgb_array"],
    }

    ACTION = ["steer", "throttle"]

    def __init__(self, level, time_step=0.05):
        # start simulation subprocess
        self.viewer = DonkeyUnitySim(level, time_step)

        # steering and throttle
        self.action_space = spaces.Discrete(len(self.ACTION))

        # camera sensor data
        self.observation_space = spaces.Box(0, 255, self.viewer.get_sensor_size())

        # simulation related variables.
        self._seed()

        #start donkey sim thread listener
        self.thread = Thread(target=self.viewer.listen)
        self.thread.start()

        self.reset()

    def _seed(self, seed=None):
        self.np_random, seed = seeding.np_random(seed)
        return [seed]

    def _step(self, action):
        self.self.viewer.take_action(action)
        observation, reward, done, info = self.viewer.observe()
        return observation, reward, done, info

    def _reset(self):
        self.viewer.reset()
        observation, reward, done, info = self.viewer.observe()
        return observation

    def _render(self, mode="human", close=False):
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
