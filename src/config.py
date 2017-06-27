#Some details about our camera image
image_width = 256
image_height = 256
image_depth = 3

def get_camera_image_dim():
    return (image_depth, image_height, image_width)

# In previous versions, we set the image transposed in Theano order.
# When training new models users can change this to be 
# False to use Tensorflow recommended channel order.
# This has a large impact on the size of trainable weights of the model.
image_transposed = False

def get_input_shape():
    '''
    Get the camera image dimension and then check the image_transposed
    flag to return the channels in the right order.
    '''
    ch, row, col = get_camera_image_dim()

    if image_transposed:
        input_shape = (ch, row, col)
    else:
        input_shape = (col, row, ch)

    return input_shape
    

def is_model_image_input_transposed(model):
    '''
    Check the model input shape to see if the depth is the first dimension.
    That usually indicates that the image was transposed on training.
    '''
    s = model.layers[0].input[0].get_shape()
    return s[0] == image_depth