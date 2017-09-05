'''
Models
Define the different NN models we will use
Author: Tawn Kramer
'''
from __future__ import print_function
from keras.models import Sequential
from keras.layers import Conv2D, MaxPooling2D
from keras.layers import Dense, Lambda, ELU
from keras.layers import Activation, Dropout, Flatten, Dense
from keras.layers import Cropping2D
from keras.optimizers import Adadelta

import conf

def show_model_summary(model):
    model.summary()
    for layer in model.layers:
        print(layer.output_shape)

def get_nvidia_model(num_outputs):
    '''
    this model is inspired by the NVIDIA paper
    https://images.nvidia.com/content/tegra/automotive/images/2016/solutions/pdf/end-to-end-dl-using-px.pdf
    Activation is ELU
    '''
    row, col, ch = conf.row, conf.col, conf.ch
    
    model = Sequential()

    model.add(Cropping2D(cropping=((60,0), (0,0)), input_shape=(row, col, ch)))

    model.add(Lambda(lambda x: x/127.5 - 1.))
    model.add(Conv2D(24, (5, 5), strides=(2, 2), padding="same"))
    model.add(ELU())
    model.add(Conv2D(32, (5, 5), strides=(2, 2), padding="same"))
    model.add(ELU())
    model.add(Conv2D(64, (5, 5), strides=(2, 2), padding="same"))
    model.add(ELU())
    model.add(Conv2D(64, (3, 3), strides=(2, 2), padding="same"))
    model.add(ELU())
    model.add(Dropout(.5))
    model.add(Conv2D(64, (3, 3), strides=(1, 1), padding="same"))
    model.add(Flatten())
    model.add(Dense(1000))
    model.add(Dropout(.1))
    model.add(ELU())
    model.add(Dense(128))
    model.add(ELU())
    model.add(Dense(num_outputs))

    model.compile(optimizer=Adadelta(), loss="mse")
    return model
