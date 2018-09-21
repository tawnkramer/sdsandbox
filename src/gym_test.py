import gym
import donkey_gym

NUM_EPISODES = 3
MAX_T = 1000

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

def select_action():
    return (0.1, 0.1)

if __name__ == "__main__":

    # Initialize the donkey environment
    env = gym.make("donkey-warehouse-v0")