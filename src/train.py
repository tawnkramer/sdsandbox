'''
Train
Start the training and validation servers in their own subprocess. Run the steering training in this process.
It can sometimes take multiple CTRL+C breaks to stop this.
Author: Tawn Kramer
'''
import multiprocessing as mp
import server
import train_steering_model

def train(output, datadir, tprefix, vprefix, resume):
    try:
        #first start training server
        ts = mp.Process( name = 'training server', target=server.run_default_train_server, args=(datadir, tprefix))
        ts.start()

        #then start validation server
        vs = mp.Process( name = 'validation server', target=server.run_default_validation_server, args=(datadir, vprefix))
        vs.start()

        #then start training
        train_steering_model.run_default_training(output, resume)
    except KeyboardInterrupt:
        print 'stopping'


if __name__ == "__main__":
    import argparse

    # Parameters
    parser = argparse.ArgumentParser(description='Prepare training data from logs and images')
    parser.add_argument('--train-prefix', dest='tprefix', default='train_', help='prefix of training data')
    parser.add_argument('--val-prefix', dest='vprefix', default='val_', help='prefix of validation data')
    parser.add_argument('--data-dir', dest='datadir', default='../dataset', help='dir containing camera and log datasets')
    parser.add_argument('--output', dest='output', default='./outputs/steering_model/highway', help='output file.')
    parser.add_argument('--resume', dest='resume', action='store_true', help='Optional flag to resume training.')

    args, more = parser.parse_known_args()
    
    train(args.output, args.datadir, args.tprefix, args.vprefix, args.resume)
