'''
Models
Define the different NN models we will use
Author: Tawn Kramer
'''
from __future__ import print_function
from tensorflow.keras.models import Model
from tensorflow.keras.layers import Conv2D, MaxPooling2D, Input
from tensorflow.keras.layers import Dense, Lambda, ELU
from tensorflow.keras.layers import Activation, Dropout, Flatten, Dense
from tensorflow.keras.layers import Cropping2D
from tensorflow.keras.optimizers import Adadelta, Adam

import conf

def show_model_summary(model):
    model.summary()
    for layer in model.layers:
        print(layer.output_shape)

def get_nvidia_model(num_outputs):
    '''
    this model is inspired by the NVIDIA paper
    https://images.nvidia.com/content/tegra/automotive/images/2016/solutions/pdf/end-to-end-dl-using-px.pdf
    Activation is RELU
    '''
    row, col, ch = conf.row, conf.col, conf.ch
    
    drop = 0.1
    
    img_in = Input(shape=(row, col, ch), name='img_in')
    x = img_in
    #x = Cropping2D(cropping=((10,0), (0,0)))(x) #trim 10 pixels off top
    #x = Lambda(lambda x: x/127.5 - 1.)(x) # normalize and re-center
    x = Lambda(lambda x: x/255.0)(x)
    x = Conv2D(24, (5,5), strides=(2,2), activation='relu', name="conv2d_1")(x)
    x = Dropout(drop)(x)
    x = Conv2D(32, (5,5), strides=(2,2), activation='relu', name="conv2d_2")(x)
    x = Dropout(drop)(x)
    x = Conv2D(64, (5,5), strides=(2,2), activation='relu', name="conv2d_3")(x)
    x = Dropout(drop)(x)
    x = Conv2D(64, (3,3), strides=(1,1), activation='relu', name="conv2d_4")(x)
    x = Dropout(drop)(x)
    x = Conv2D(64, (3,3), strides=(1,1), activation='relu', name="conv2d_5")(x)
    x = Dropout(drop)(x)
    
    x = Flatten(name='flattened')(x)
    x = Dense(100, activation='relu')(x)
    #x = Dropout(drop)(x)
    x = Dense(50, activation='relu')(x)
    #x = Dropout(drop)(x)

    outputs = []
    outputs.append(Dense(num_outputs, activation='linear', name='steering_throttle')(x))
    
        
    model = Model(inputs=[img_in], outputs=outputs)
    opt = Adam(lr=0.0001)
    model.compile(optimizer=opt, loss="mse", metrics=['acc'])
    return model
