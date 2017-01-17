# SdSandbox

Self Driving Car Sandbox


[![IMAGE ALT TEXT](https://img.youtube.com/vi/e0AFMilaeMI/0.jpg)](https://www.youtube.com/watch?v=e0AFMilaeMI "self driving car sim")


## Summary

Use Unity 3d game engine to simulate car physics in a 3d world. 
Generate image steering pairs to train a neural network. Uses comma ai training code with NVidia NN topology.
Then validate the steering control by sending images to your neural network and feed steering back into the simulator to drive.

## Some videos to help you get started

### Downloading and training your first network
[![IMAGE ALT TEXT](https://img.youtube.com/vi/oe7fYuYw8GY/0.jpg)](https://www.youtube.com/watch?v=oe7fYuYw8GY "Getting Started w sdsandbox")

### World beautification
[![IMAGE ALT TEXT](https://img.youtube.com/vi/FhAKaH3ysow/0.jpg)](https://www.youtube.com/watch?v=FhAKaH3ysow "Making a more interesting world.")

## Setup

You need to have [Unity](https://unity3d.com/get-unity/download) installed, and all python modules listed in the Requirements section below.



## For the impatient ( I just want to see this work )

1) Start the prediction server with the pre-trained model. 

```bash
cd sdsandbox/src
python predict_server.py usc
```

2) Load the Unity project sdsandbox/sdsim in Unity. Double click on Assets/Scenes/UnityStandardCar to open that scene.  

3) Hit the start button to launch. Then the "Use NN Steering".  


#To create your own data and train

## Generate training data

1) Load the Unity project sdsandbox/sdsim in Unity.  

2) Create a dir sdsandbox/sdsim/log.  

3) Hit the start arrow in Unity to launch project.  

4) Hit button "Generate Training Data" to generate image and steering training data. See sdsim/log for output files.  

5) Stop Unity sim by clicking run arrow again.  

6) Run this python script to prepare raw data for training:  

```bash
cd sdsandbox/src
python prepare_data.py --clean
```

7) Repeat 4, 5, 6 until you have lots of training data. 30gb+ is good. On your last run, prepare a validation set:  

```bash
python prepare_data.py --validation --clean
```



## Train Neural network

```bash
python train.py mymodel
```

Let this run. It may take 12+ hours if running on CPU.  



## Run car with NN

1) Start the prediction server. This listens for images and returns a steering result.  

```bash
python predict_server.py mymodel
```

2) Start Unity project sdsim  

3) Push button "Use NN Steering"  



## Requirements
[python 2.7 64 bit](https://www.python.org/)  
[tensorflow-0.9](https://github.com/tensorflow/tensorflow)  
[keras-1.0.6](https://github.com/fchollet/keras)   
[h5](http://www.h5py.org/)  
[pillow](https://python-pillow.org/)  
[Unity 5.5+](https://unity3d.com/get-unity/download)  

## Credits

Tawn Kramer, Riccardo Biasini, George Hotz, Sam Khalandovsky, Eder Santana, and Niel van der Westhuizen  

