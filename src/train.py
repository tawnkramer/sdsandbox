'''
Train
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
import json

import numpy as np
from PIL import Image
from tensorflow import keras

import conf
import models

'''
matplotlib can be a pain to setup. So handle the case where it is absent. When present,
use it to generate a plot of training results.
'''
try:
    import matplotlib
    # Force matplotlib to not use any Xwindows backend.
    matplotlib.use('Agg')

    import matplotlib.pyplot as plt
    do_plot = True
except:
    do_plot = False


def shuffle(samples):
    '''
    randomly mix a list and return a new list
    '''
    ret_arr = []
    len_samples = len(samples)
    while len_samples > 0:
        iSample = random.randrange(0, len_samples)
        ret_arr.append(samples[iSample])
        del samples[iSample]
        len_samples -= 1
    return ret_arr

def load_json(filename):
    with open(filename, "rt") as fp:
        data = json.load(fp)
    return data

def generator(samples, batch_size=64,):
    '''
    Rather than keep all data in memory, we will make a function that keeps
    it's state and returns just the latest batch required via the yield command.
    
    As we load images, we can optionally augment them in some manner that doesn't
    change their underlying meaning or features. This is a combination of
    brightness, contrast, sharpness, and color PIL image filters applied with random
    settings. Optionally a shadow image may be overlayed with some random rotation and
    opacity.
    We flip each image horizontally and supply it as a another sample with the steering
    negated.
    '''
    num_samples = len(samples)
    
    while 1: # Loop forever so the generator never terminates
        samples = shuffle(samples)
        #divide batch_size in half, because we double each output by flipping image.
        for offset in range(0, num_samples, batch_size):
            batch_samples = samples[offset:offset+batch_size]
            
            images = []
            controls = []
            for fullpath in batch_samples:
                try:
                
                    frame_number = os.path.basename(fullpath).split("_")[0]
                    json_filename = os.path.join(os.path.dirname(fullpath), "record_" + frame_number + ".json")
                    data = load_json(json_filename)
                    steering = float(data["user/angle"])
                    throttle = float(data["user/throttle"])
                
                    try:
                        image = Image.open(fullpath)
                    except:
                        print('failed to open', fullpath)
                        continue

                    #PIL Image as a numpy array
                    image = np.array(image, dtype=np.float32)

                    images.append(image)
                    
                    if conf.num_outputs == 2:
                        controls.append([steering, throttle])
                    elif conf.num_outputs == 1:
                        controls.append([steering])
                    else:
                        print("expected 1 or 2 outputs")

                except Exception as e:
                    print(e)
                    print("we threw an exception on:", fullpath)
                    yield [], []


            # final np array to submit to training
            X_train = np.array(images)
            y_train = np.array(controls)
            yield X_train, y_train


def get_files(filemask):
    '''
    use a filemask and search a path recursively for matches
    '''
    #matches = glob.glob(os.path.expanduser(filemask))
    #return matches
    filemask = os.path.expanduser(filemask)
    path, mask = os.path.split(filemask)
    
    matches = []
    for root, dirnames, filenames in os.walk(path):
        for filename in fnmatch.filter(filenames, mask):
            matches.append(os.path.join(root, filename))
    return matches


def train_test_split(lines, test_perc):
    '''
    split a list into two parts, percentage of test used to seperate
    '''
    train = []
    test = []

    for line in lines:
        if random.uniform(0.0, 1.0) < test_perc:
            test.append(line)
        else:
            train.append(line)

    return train, test

def make_generators(inputs, limit=None, batch_size=64):
    '''
    load the job spec from the csv and create some generator for training
    '''
    
    #get the image/steering pairs from the csv files
    lines = get_files(inputs)
    print("found %d files" % len(lines))

    if limit is not None:
        lines = lines[:limit]
        print("limiting to %d files" % len(lines))
    
    train_samples, validation_samples = train_test_split(lines, test_perc=0.2)

    print("num train/val", len(train_samples), len(validation_samples))
    
    # compile and train the model using the generator function
    train_generator = generator(train_samples, batch_size=batch_size)
    validation_generator = generator(validation_samples, batch_size=batch_size)
    
    n_train = len(train_samples)
    n_val = len(validation_samples)
    
    return train_generator, validation_generator, n_train, n_val


def go(model_name, epochs=50, inputs='./log/*.jpg', limit=None):

    print('working on model', model_name)

    '''
    modify config.json to select the model to train.
    '''
    model = models.get_nvidia_model(conf.num_outputs)

    '''
    display layer summary and weights info
    '''
    #models.show_model_summary(model)

    callbacks = [
        keras.callbacks.EarlyStopping(monitor='val_loss', patience=conf.training_patience, verbose=0),
        keras.callbacks.ModelCheckpoint(model_name, monitor='val_loss', save_best_only=True, verbose=0),
    ]
    
    batch_size = conf.training_batch_size


    #Train on session images
    train_generator, validation_generator, n_train, n_val = make_generators(inputs, limit=limit, batch_size=batch_size)

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
    
    try:
        if do_plot:
            # summarize history for loss
            plt.plot(history.history['loss'])
            plt.plot(history.history['val_loss'])
            plt.title('model loss')
            plt.ylabel('loss')
            plt.xlabel('epoch')
            plt.legend(['train', 'test'], loc='upper left')
            plt.savefig('loss.png')
    except:
        print("problems with loss graph")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='train script')
    parser.add_argument('--model', type=str, help='model name')
    parser.add_argument('--epochs', type=int, default=conf.training_default_epochs, help='number of epochs')
    parser.add_argument('--inputs', default='../dataset/log/*.jpg', help='input mask to gather images')
    parser.add_argument('--limit', type=int, default=None, help='max number of images to train with')
    args = parser.parse_args()
    
    go(args.model, epochs=args.epochs, limit=args.limit, inputs=args.inputs)

#python train.py ..\outputs\mymodel_aug_90_x4_e200 --epochs=200
