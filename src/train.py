'''
train.py
Train your nerual network
Author: Tawn Kramer
'''
from __future__ import print_function
import os
import sys
import glob
import time
import fnmatch
import argparse
import random

import numpy as np
from PIL import Image
from tensorflow.python import keras
import matplotlib.pyplot as plt

import models
import conf

def parse_img_filepath(filepath):
    '''
    get the steering and throttle value from the filename of the image.
    '''
    basename = os.path.basename(filepath)

    #less .jpg
    f = basename[:-4]
    f = f.split('_')

    throttle = float(f[3])
    steering = float(f[5])
    
    data = {'steering':steering, 'throttle':throttle }
    return data


def generator(samples, batch_size=32):
    '''
    Rather than keep all data in memory, we will make a function that keeps
    it's state and returns just the latest batch required via the yield command.   
    '''
    num_samples = len(samples)
    
    while True:

        random.shuffle(samples)
        
        for offset in range(0, num_samples, batch_size):
            batch_samples = samples[offset:offset+batch_size]
            
            images = []
            controls = []
            for fullpath in batch_samples:
                data = parse_img_filepath(fullpath)
            
                steering = data["steering"]
                throttle = data["throttle"]

                image = Image.open(fullpath)

                #PIL Image as a numpy array
                image = np.array(image)
                images.append(image)
                
                if conf.num_outputs == 2:
                    controls.append([steering, throttle])
                elif conf.num_outputs == 1:
                    controls.append([steering])
                else:
                    raise("expected 1 or 2 ouputs")

            # final np array to submit to training
            X_train = np.array(images)
            y_train = np.array(controls)
            yield X_train, y_train


def get_files(filemask):
    '''
    use a filemask and search a path recursively for matches
    '''
    path, mask = os.path.split(filemask)
    matches = []
    for root, dirnames, filenames in os.walk(path):
        for filename in fnmatch.filter(filenames, mask):
            matches.append(os.path.join(root, filename))
    return matches


def train_test_split(files, test_perc):
    '''
    split a list into two parts, percentage of test used to seperate
    '''
    train = []
    test = []

    for filename in files:
        if random.random() < test_perc:
            test.append(filename)
        else:
            train.append(filename)

    return train, test


def make_generators(inputs, batch_size=32):
    '''
    create some generator for training
    '''
    
    #get the list of images
    files = get_files(inputs)
    print("found %d files" % len(files))

    train_samples, validation_samples = train_test_split(files, test_perc=0.2)

    print("num train/val", len(train_samples), len(validation_samples))
    
    # compile and train the model using the generator function
    train_generator = generator(train_samples, batch_size=batch_size)
    validation_generator = generator(validation_samples, batch_size=batch_size)
    
    n_train = len(train_samples)
    n_val = len(validation_samples)
    
    return train_generator, validation_generator, n_train, n_val


def go(model_name, epochs=50, inputs='./log/*.jpg'):

    print('working on model', model_name)

    '''
    modify config.json to select the model to train.
    '''
    model = models.get_model()

    '''
    display layer summary and weights info
    '''
    models.show_model_summary(model)

    callbacks = [
        keras.callbacks.EarlyStopping(monitor='val_loss', patience=conf.training_patience, verbose=0),
        keras.callbacks.ModelCheckpoint(model_name, monitor='val_loss', save_best_only=True, verbose=0),
    ]
    
    batch_size = conf.training_batch_size

    #Train on session images
    train_generator, validation_generator, n_train, n_val = make_generators(inputs, batch_size=batch_size)

    if n_train == 0:
        print('no training data found')
        return

    steps_per_epoch = n_train // batch_size
    validation_steps = n_val // batch_size

    print("steps_per_epoch", steps_per_epoch, "validation_steps", validation_steps)

    history = model.fit_generator(train_generator, 
        steps_per_epoch = steps_per_epoch,
        validation_data = validation_generator,
        validation_steps = validation_steps,
        epochs=epochs,
        verbose=1,
        callbacks=callbacks)
    
    # summarize history for loss
    plt.plot(history.history['loss'])
    plt.plot(history.history['val_loss'])
    plt.title('model loss')
    plt.ylabel('loss')
    plt.xlabel('epoch')
    plt.legend(['train', 'test'], loc='upper left')
    plt.show()

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='train script')
    parser.add_argument('model', type=str, help='model name')
    parser.add_argument('--epochs', type=int, default=conf.training_default_epochs, help='number of epochs')
    parser.add_argument('--inputs', default='dataset/log/*.jpg', help='input mask to gather images')
    args = parser.parse_args()
    
    go(args.model, epochs=args.epochs, inputs=args.inputs)

