import math

training_patience = 10
training_batch_size = 64
training_default_epochs = 100

image_width = 160
image_height = 120
image_depth = 3

#when we wish to try training for steering and throttle:
num_outputs = 2

#when steering alone:
#num_outputs = 1

throttle_out_scale = 10.0
