'''
Convenience script to make a copy of the model.
'''
import os
import sys
import shutil

o_train = sys.argv[1]
n_train = sys.argv[2]

base_path = '../outputs/steering_model/'

#copy json
src = os.path.join(base_path, o_train + '.json')
dest = os.path.join(base_path, n_train + '.json')
shutil.copyfile(src, dest)

#copy keras
src = os.path.join(base_path, o_train + '.keras')
dest = os.path.join(base_path, n_train + '.keras')
shutil.copyfile(src, dest)

