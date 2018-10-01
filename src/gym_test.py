import os
import argparse
import gym
import donkey_gym
import time
import random

NUM_EPISODES = 3
MAX_T = 1000

def select_action():
    return ( random.random() * 2.0 - 1.0, 0.3)

def simulate(env):

    for episode in range(NUM_EPISODES):

        # Reset the environment
        obv = env.reset()

        for t in range(MAX_T):
            # Select an action
            action = select_action()

            # execute the action
            obv, reward, done, _ = env.step(action)

            if done:
                break

if __name__ == "__main__":

    parser = argparse.ArgumentParser(description='gym_test')
    parser.add_argument('--sim', type=str, default="default", help='path to unity simulator. maybe be left at manual if you would like to start the sim on your own.')
    parser.add_argument('--headless', type=int, default=0, help='1 to supress graphics')
    parser.add_argument('--port', type=int, default=9091, help='port to use for websockets')
    

    args = parser.parse_args()

    if args.sim == "default":
        args.sim = 'sdsim\Build\DonkeySimWindows\DonkeySim.exe'

    #we pass arguments to the donkey_gym init via these
    os.environ['DONKEY_SIM_PATH'] = args.sim
    os.environ['DONKEY_SIM_PORT'] = str(args.port)
    os.environ['DONKEY_SIM_HEADLESS'] = str(args.headless)

    # Initialize the donkey environment
    #env = gym.make("donkey-warehouse-v0")
    env = gym.make("donkey-generated-track-v0")

    simulate(env)

    env.close()
