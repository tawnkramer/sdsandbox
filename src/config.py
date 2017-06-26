#Some details about our camera

def get_camera_image_dim():
    return (3, 256, 256)

#in previous versions, we set the image transposed in Theano order.
#it can be changed to True if needed to be compatible with older models.
image_tranposed = False

def get_input_shape():
    ch, row, col = get_camera_image_dim()

    if image_tranposed:
        input_shape = (ch, row, col)
    else:
        input_shape = (col, row, ch)
    return input_shape
    