#!/usr/bin/env python
'''
Prepare Data
Move log data into place for training
Author: Tawn Kramer
'''
from __future__ import print_function
import os
import time
import glob
import shutil


def prepare(src_path, dest_path):

    #make a new filename that uses the last modified time stamp
    #on the dir with the driving log. replace illegal filename characers.
    src_basename, filemask = os.path.split(src_path)
    t = time.ctime(os.path.getmtime(src_basename))
    t = t.replace(' ', '_')
    t = t.replace(':', '_')


    #we save the steering, and other single channel data to log, images to camera
    basepath_log = os.path.join(dest_path, "log")

    #make sure these paths exist so the file open will succeed
    if not os.path.exists(basepath_log):
        os.makedirs(basepath_log)

    log_path = os.path.join(basepath_log, "logs_" + t)
    os.makedirs(log_path)
    
    #do a directery listing of all images
    print('gathering images', src_path)
    images = glob.glob(src_path)
    
    for img_filename in images:
        path, name = os.path.split(img_filename)
        dest_fnm = os.path.join(log_path, name)
        shutil.move(img_filename, dest_fnm)

if __name__ == "__main__":
    import argparse

    # Parameters
    parser = argparse.ArgumentParser(description='Prepare training data from logs and images')
    parser.add_argument('--log-src', dest='log_src', default='../sdsim/log/*.*', help='path to log data')
    parser.add_argument('--out-path', dest='out_path', default='../dataset/', help='path for output.')
    
    args, more = parser.parse_known_args()

    prepare(args.log_src, args.out_path)
