# sdsandbox

## Train

Use Unity 3d game engine to sumlate car physics in a simulated world. 
Generate image steering pairs to train a neural network. 
Then use the Unity engine to validate the steering control by sending images to your neural network and using steering output to drive.

## Downloading Unity

need to have Unity installed [Unity Webset](https://unity3d.com/get-unity/download)

## Generate training data

load the Unity project in sdsandbox/sdunity
generate training data
cd sdsandbox/src
python prepare_data.py --clean

## Train Neural network

cd sdsandbox/src
python train.py highway


## Run car with NN
cd sdsandbox/src
python predict_server.py highway.json 
start Unity sdunity
run in network steering mode


## Requirements
[tensorflow-0.9](https://github.com/tensorflow/tensorflow)  
[keras-1.0.6](https://github.com/fchollet/keras)  
[cv2](https://anaconda.org/menpo/opencv3)

## Credits

Riccardo Biasini, George Hotz, Sam Khalandovsky, Eder Santana, and Niel van der Westhuizen
