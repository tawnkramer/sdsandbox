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
from stable_baselines.common.vec_env import DummyVecEnv, SubprocVecEnv
from stable_baselines.common import set_global_seeds
from stable_baselines import PPO2

def make_env(env_id, rank, seed=0):
    """
    Utility function for multiprocessed env.
    
    :param env_id: (str) the environment ID
    :param num_env: (int) the number of environment you wish to have in subprocesses
    :param seed: (int) the inital seed for RNG
    :param rank: (int) index of the subprocess
    """
    def _init():
        env = gym.make(env_id)
        env.seed(seed + rank)
        env.reset()
        return env
    set_global_seeds(seed)
    return _init


if __name__ == "__main__":

    parser = argparse.ArgumentParser(description='gym_test')
    parser.add_argument('--sim', type=str, default="default", help='path to unity simulator. maybe be left at manual if you would like to start the sim on your own.')
    parser.add_argument('--headless', type=int, default=0, help='1 to supress graphics')
    parser.add_argument('--port', type=int, default=9091, help='port to use for websockets')
    parser.add_argument('--test', action="store_true", help='load the model and play')
    

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

    env_id = "donkey-generated-track-v0"

    if args.test:
        env = gym.make(env_id)
        env = DummyVecEnv([lambda: env])

        model = PPO2.load("ppo_donkey")
    
        obs = env.reset()
        for i in range(1000):
            action, _states = model.predict(obs)
            obs, rewards, dones, info = env.step(action)
            env.render()

        print("done testing")
        
    else:

        num_cpu = 4  # Number of processes to use
        # Create the vectorized environment
        env = SubprocVecEnv([make_env(env_id, i) for i in range(num_cpu)])

        model = PPO2(MlpPolicy, env, verbose=1)
        model.learn(total_timesteps=10000)

        obs = env.reset()
        for i in range(1000):
            action, _states = model.predict(obs)
            obs, rewards, dones, info = env.step(action)
            env.render()

        # Save the agent
        model.save("ppo_donkey")
        print("done training")

    env.close()
