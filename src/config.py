#Some details about our camera
image_width = 256
image_height = 256
image_depth = 3

def get_camera_image_dim():
    return (image_depth, image_height, image_width)

#in previous versions, we set the image transposed in Theano order.
#it is True to be compatible with older models. but when training new models
#users can change this to be False to use Tensorflow recommended channel order.
image_tranposed = False

def get_input_shape():
    ch, row, col = get_camera_image_dim()

    if image_tranposed:
        input_shape = (ch, row, col)
    else:
        input_shape = (col, row, ch)
    return input_shape
    
def is_model_image_input_transposed(model):
    s = model.layers[0].input[0].get_shape()
    return s[0] == image_depth;