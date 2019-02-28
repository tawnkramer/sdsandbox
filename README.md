# SdSandbox

Self Driving Car Sandbox


[![IMAGE ALT TEXT](https://img.youtube.com/vi/e0AFMilaeMI/0.jpg)](https://www.youtube.com/watch?v=e0AFMilaeMI "self driving car sim")


## Summary

Use Unity 3d game engine to simulate car physics in a 3d world. 
Generate image steering pairs to train a neural network. Uses NVidia PilotNet NN topology.
Then validate the steering control by sending images to your neural network and feed steering back into the simulator to drive.

## Some videos to help you get started

### Training your first network
[![IMAGE ALT TEXT](https://img.youtube.com/vi/oe7fYuYw8GY/0.jpg)](https://www.youtube.com/watch?v=oe7fYuYw8GY "Getting Started w sdsandbox")

### World complexity
[![IMAGE ALT TEXT](https://img.youtube.com/vi/FhAKaH3ysow/0.jpg)](https://www.youtube.com/watch?v=FhAKaH3ysow "Making a more interesting world.")

### Creating a robust training set

[![IMAGE ALT TEXT](https://img.youtube.com/vi/_h8l7qoT4zQ/0.jpg)](https://www.youtube.com/watch?v=_h8l7qoT4zQ "Creating a robust sdc.")

## Setup

You need to have [Unity](https://unity3d.com/get-unity/download) installed, and all python modules listed in the Requirements section below.

Linix Unity install [here](https://forum.unity3d.com/threads/unity-on-linux-release-notes-and-known-issues.350256/). Check last post in this thread.

You need python 3.4 or higher, 64 bit. You can create a virtual env if you like:
```bash
virtualenv -p python3 env
source env/bin/activate
```

And then you can install the requirements. 
```bash
pip install -r requirements.txt
```

If you have an cuda supported GPU - probably NVidia
```bash
pip install tensorflow-gpu
```

Or without a supported gpu
```bash
pip install tensorflow
```


## Demo

1) Start the prediction server with the pre-trained model. 

```bash
cd sdsandbox
python src/predict_server.py outputs/highway.h5
```
 If you get a crash loading this model, you will not be able to run the demo. But you can still generate your own model. This is a problem between tensorflow/keras versions. 
 
2) Load the Unity project sdsandbox/sdsim in Unity. Double click on Assets/Scenes/main to open that scene.  

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
cd sdsandbox
python src/prepare_data.py --src=sdsim/log/*.* --dest=dataset
```

7) Repeat 4, 5, 6 until you have lots of training data. Over 50K images for best results.



## Train Neural network

```bash
python src/train.py outputs/mymodel.h5
```

Let this run. It may take many minutes if running on CPU. A GPU is faster, but requires you setup tensorflow-gpu.



## Run car with NN

1) Start the prediction server. This listens for images and returns a steering result.  

```bash
python src/predict_server.py --model=outputs/mymodel.h5
```

2) Start Unity project sdsim  

3) Push button "Use NN Steering"  



## Requirements
* [python 3.6+ 64 bit](https://www.python.org/)*
* [tensorflow-1.12+](https://github.com/tensorflow/tensorflow)  
* [pillow](https://python-pillow.org/)  
* [Unity](https://unity3d.com/get-unity/download)  



## Credits

Tawn Kramer  

