import socket
import argparse
import sys
import numpy as np
import h5py
import json
import time

# ***** main loop *****
if __name__ == "__main__":
    print 'started'
    parser = argparse.ArgumentParser(description='predict client')
    parser.add_argument('--dataset', type=str, default="tawn_Thu_Dec_29_15_52_43_2016", help='Dataset/video clip name')
    args = parser.parse_args()
    # default dataset is the validation data on the highway
    dataset = args.dataset
    cam = h5py.File("dataset/camera/"+dataset+".h5", "r")
    iFrame = 1
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.connect(('localhost', 9090))
    ch, row, col = 3, 256, 256  # camera format - todo read it in!
    while iFrame < 2:
        img = cam['X'][iFrame] #first image for now.
        bytes = img.tobytes()
        img_specs = '{ "num_bytes" : %d, "width" : %d, "height" : %d, "num_channels" : %d, "format" : "array_of_channels" , "flip_y" : 0 }' % ( len(bytes), row, col, ch )
        start = time.time()
        sock.send(img_specs)
        print 'sent:', img_specs
        data = sock.recv(1024)
        print 'got', data
        if data == "{ 'response' : 'ready_for_image' }":
            print 'sending image to classify', len(bytes), 'bytes'
            sock.send(bytes)
        else:
            print 'wasnt expecting:', data
        recv = sock.recv(1024)
        print 'got', recv
        duration = time.time() - start
        print 'took', duration, 'sec'
        iFrame += 1
    sock.close()
else:
    print 'not main'
