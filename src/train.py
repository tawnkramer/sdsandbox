'''
Train
Start the training and validation servers in their own subprocess. Run the steering training in this process.
Author: Tawn Kramer
'''
import multiprocessing as mp
import server
import train_steering_model

def train(model):
    #first start training server
    ts = mp.Process( name = 'training server', target=server.run_default_train_server)
    ts.start()

    #then start validation server
    vs = mp.Process( name = 'validation server', target=server.run_default_validation_server)
    vs.start()

    #then start training
    train_steering_model.run_default_training(model)


if __name__ == "__main__":
    import argparse

    # Parameters
    parser = argparse.ArgumentParser(description='Prepare training data from logs and images')
    parser.add_argument('--model', dest='model', default='../outputs/steering_model/highway', help='path to model')

    train(args.model)
