'''
file: ppo_train.py
author: Tawn Kramer
date: 13 October 2018
notes: ppo2 test from stable-baselines here:
https://github.com/hill-a/stable-baselines
'''
import os
import argparse
import gym
import donkey_gym

from stable_baselines.common.policies import MlpPolicy
from stable_baselines.common.vec_env import DummyVecEnv
from stable_baselines import PPO2

if __name__ == "__main__":

    parser = argparse.ArgumentParser(description='gym_test')
    parser.add_argument('--sim', type=str, default="default", help='path to unity simulator. maybe be left at manual if you would like to start the sim on your own.')
    parser.add_argument('--headless', type=int, default=0, help='1 to supress graphics')
    parser.add_argument('--port', type=int, default=9091, help='port to use for websockets')
    parser.add_argument('--play', action="store_true", help='load the model and play')
    

    args = parser.parse_args()

    if args.sim == "default":
        if args.headless == 1:
            args.sim = '../sdsim/build/DonkeySimLinux/donkey_sim_headless.x86_64'
        else:
            args.sim = '../sdsim/build/DonkeySimLinux/donkey_sim.x86_64'

    #we pass arguments to the donkey_gym init via these
    os.environ['DONKEY_SIM_PATH'] = args.sim
    os.environ['DONKEY_SIM_PORT'] = str(args.port)
    os.environ['DONKEY_SIM_HEADLESS'] = str(args.headless)

    env = gym.make('donkey-generated-track-v0')
    env = DummyVecEnv([lambda: env])  # The algorithms require a vectorized environment to run

    if args.play:
        model = PPO2.load("ppo_donkey")
    
        obs = env.reset()
        for i in range(1000):
            action, _states = model.predict(obs)
            obs, rewards, dones, info = env.step(action)
            env.render()
        
    else:
        model = PPO2(MlpPolicy, env, verbose=1)
        model.learn(total_timesteps=10000)

        obs = env.reset()
        for i in range(1000):
            action, _states = model.predict(obs)
            obs, rewards, dones, info = env.step(action)
            env.render()

        # Save the agent
        model.save("ppo_donkey")

    env.close()
