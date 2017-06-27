#!/usr/bin/env python
'''
Prepare Data
Take the raw images and steering data and bundle them up in h5 files for training.
Author: Tawn Kramer
'''
from __future__ import print_function
import h5py
import numpy as np
import os
import time
import glob
from PIL import Image
import pdb
import config

def prepare(drivinglog, drivingimages, outputpath, prefix, activity):
    #make a new filename that uses the last modified time stamp
    #on the dir with the driving log. replace illegal filename characers.
    t = time.ctime(os.path.getmtime(drivinglog))
    t = t.replace(' ', '_')
    t = t.replace(':', '_')
    basename = prefix + t + ".h5"

    #we save the steering, and other single channel data to log, images to camera
    basepath_log = os.path.join(outputpath, "log")
    basepath_camera = os.path.join(outputpath, "camera")

    #make sure these paths exist so the file open will succeed
    if not os.path.exists(basepath_log):
        os.makedirs(basepath_log)

    if not os.path.exists(basepath_camera):
        os.makedirs(basepath_camera)

    #open the logfile and read all lines
    outfilename = os.path.join(basepath_log, basename)
    infile = open(drivinglog, "r")
    lines = []
    for line in infile:
        lines.append(line)
    infile.close()

    #do a directery listing of all images
    print('gathering images', drivingimages)
    images = glob.glob(drivingimages)
    num_images = len(images)
    num_records = len(lines)

    #when the counts don't match, use the smaller of the two.
    if(num_images != num_records):
        if num_images < num_records:
            num_records = num_images
        else:
            num_images = num_records
            images = images[:num_images]

    print(len(lines), 'steering records')
    
    #create a h5 file for the log data, and then create two datasets within that file
    #for steering angle and speed
    logf = h5py.File(outfilename, "w")
    dse = logf.create_dataset("steering_angle", (num_records, ), dtype='float64')
    dse_speed = logf.create_dataset("speed", (num_records, ), dtype='float64')
    iIter = 0
    iLine = 0
    iRecord = 0
    num_lines = len(lines)
    log_ids = []
    while iLine < num_lines and iRecord < num_records:
        tokens = lines[iLine].split(',')
        iLine += 1
        iRecord += 1
        if len(tokens) != 4:
            continue
        log_activity = tokens[1]
        if activity is not None and activity != log_activity:
            continue;
        log_ids.append(int(tokens[0]))
        steering = float(tokens[2])
        speed = float(tokens[3])
        dse[iIter] = np.array([steering])
        dse_speed[iIter] = np.array([speed])
        iIter += 1
        if iIter % 1000 == 0:
            print(iIter)
    if activity is not None:
        print(iIter, 'records found w activity', activity)
    logf.close()

    print('done with log')

    outfilename = os.path.join(basepath_camera, basename)
    camf = h5py.File(outfilename, "w")
    print(num_images, 'images')
    ch, rows, col = config.get_camera_image_dim()
    if config.image_tranposed:
        dse = camf.create_dataset("X", (num_images, ch, rows, col), dtype='uint8')
    else:
        dse = camf.create_dataset("X", (num_images, col, rows, ch), dtype='uint8')
    images.sort()
    imgs_by_id = {}

    for img_filename in images:
        path, name = os.path.split(img_filename)
        first_zero = name.find('0')
        first_period = name.find('.')
        num_string = name[first_zero : first_period]
        id = int(num_string)
        imgs_by_id[id] = img_filename

    iIter = 0
    for id in log_ids:
        try:
            img_filename = imgs_by_id[id]
            im = Image.open(img_filename).convert('RGB')
            if im.width != col or im.height != rows:
                print ('Aborting! image:', img_filename, 'had the wrong dimension:', im.width, im.height, 'expecting', col, rows)
                #stopping because we are likely to see many of these..
                return
            if config.image_tranposed:
                imarr = np.array(im).transpose()
            else:
                imarr = np.array(im)
            dse[iIter] = imarr
        except KeyError:
            print('no image for frame', id)
        iIter = iIter + 1
        if iIter % 1000 == 0:
            print(iIter)
    camf.close()
    print('done with images')
    if activity is not None:
        print(iIter, 'images found w activity', activity)

def clean(controls_filename, images_filemask):
    os.unlink(controls_filename)
    files = glob.glob(images_filemask)
    for f in files:
        os.unlink(f)

if __name__ == "__main__":
    import argparse

    # Parameters
    parser = argparse.ArgumentParser(description='Prepare training data from logs and images')
    parser.add_argument('--log-path', dest='log_path', default='../sdsim/log', help='path to log data')
    parser.add_argument('--log-controls', dest='log_controls', default='log_car_controls.txt', help='control log filename')
    parser.add_argument('--log-images', dest='images', default='*.png', help='filemask for images')
    parser.add_argument('--out-path', dest='out_path', default='../dataset/', help='path for output.')
    parser.add_argument('--prefix', default='train_', help='prefix for output.')
    parser.add_argument('--validation', dest='validation', action='store_true', help='sets prefix for validation.')
    parser.add_argument('--clean', action='store_true', help='should we remove images and logs')
    parser.add_argument('--activity', dest='activity', default=None, help='activity prefix.')
    
    args, more = parser.parse_known_args()

    print("Argument summary:")
    print("activity:", args.activity)
    print("images:", args.images)

    controls_filename = os.path.join(args.log_path, args.log_controls)
    images_filemask = os.path.join(args.log_path, args.images)

    print('controls:', controls_filename)

    if args.validation:
        prefix = 'val_'
    else:
        prefix = args.prefix

    print("prefix:", prefix)
    print("")

    prepare(controls_filename, images_filemask, args.out_path, prefix, args.activity)

    if args.clean:
        clean(controls_filename, images_filemask)
