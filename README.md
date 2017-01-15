# sdsandbox

Self Driving Sandbox

## Train

Use Unity 3d game engine to sumlate car physics in a simulated world. 
Generate image steering pairs to train a neural network. 
Then use the Unity engine to validate the steering control by sending images to your neural network and using steering output to drive.



## Downloading Unity

You need to have Unity installed.  [Unity Website](https://unity3d.com/get-unity/download)


## Generate training data

1) Load the Unity project sdsandbox/sdunity in Unity. 
2) Create a dir sdsandbox/sdunity/log.
3) Hit the start arrow in Unity to launch project. 
4) Hit button to generate raw training data.
5) Stop Unity sim by clicking arrow again. Make sure to capture at least 10,000 images.
6) Run this python script to prepare raw data for training:

```bash
cd sdsandbox/src
python prepare_data.py --clean
```

7) Repeat 4, 5, 6 until you have lots of training data. 30gb+ is good. On your last run, perpare a validation set:

```bash
cd sdsandbox/src
python prepare_data.py --validation --clean
```



## Train Neural network

```bash
cd sdsandbox/src
python train.py highway
```

Let this run. It may take 12+ hours if running on CPU.


## Run car with NN

1) start the prediction server:

```bash
cd sdsandbox/src
python predict_server.py highway.json 
```
2) start Unity sdunity
3) run in network steering mode



## Requirements
[python 2.7 64 bit](https://www.python.org/)  
[tensorflow-0.9](https://github.com/tensorflow/tensorflow)  
[keras-1.0.6](https://github.com/fchollet/keras)  
[cv2](https://anaconda.org/menpo/opencv3)
[h5](http://www.h5py.org/)
[pillow](https://python-pillow.org/)
[Unity 5.5+](https://unity3d.com/get-unity/download)

## Credits

Tawn Kramer, Riccardo Biasini, George Hotz, Sam Khalandovsky, Eder Santana, and Niel van der Westhuizen
