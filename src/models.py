'''
Models
Define the different NN models we will use
Author: Tawn Kramer
'''
from __future__ import print_function
from tensorflow.python.keras.models import Sequential
from tensorflow.python.keras.layers import Conv2D, Dropout, Flatten, Dense, Cropping2D, Lambda, BatchNormalization

import conf

def show_model_summary(model):
    model.summary()
    for layer in model.layers:
        print(layer.output_shape)

def get_model(num_outputs = conf.num_outputs, input_shape = (conf.image_height, conf.image_width, conf.image_depth)):
    '''
    this model is inspired by the NVIDIA paper
    https://images.nvidia.com/content/tegra/automotive/images/2016/solutions/pdf/end-to-end-dl-using-px.pdf
    '''
    model = Sequential()

    model.add(Cropping2D(cropping=((40,0), (0,0)), input_shape=input_shape))
    drop = 0.2

    #model.add(Lambda(lambda x: x/127.5 - 1.))
    model.add(BatchNormalization())
    model.add(Conv2D(24, (5, 5), strides=(2, 2), activation="relu"))
    model.add(Dropout(drop))
    model.add(Conv2D(32, (5, 5), strides=(2, 2), activation="relu"))
    model.add(Dropout(drop))
    model.add(Conv2D(48, (5, 5), strides=(2, 2), activation="relu"))
    model.add(Dropout(drop))
    model.add(Conv2D(64, (3, 3), strides=(2, 2), activation="relu"))
    model.add(Dropout(drop))
    model.add(Conv2D(64, (3, 3), strides=(1, 1), activation="relu"))    
    model.add(Dropout(drop))
    model.add(Flatten())
    model.add(Dense(100, activation="relu"))
    model.add(Dropout(drop))
    model.add(Dense(50, activation="relu"))
    model.add(Dropout(drop))
    model.add(Dense(num_outputs))

    model.compile(optimizer="adam", loss="mse")
    return model
