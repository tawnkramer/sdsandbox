# Steering Angle model
Follow the instructions to train a deep neural network for self-steering cars.
This experiment is similar to [End to End Learning for Self-Driving
Cars](https://arxiv.org/abs/1604.07316).

These is the instructions for running each training component individually for more control:

1) Start training data server in the first terminal session
```bash
./server.py --batch 200 --port 5557
```  

2) Start validation data server in a second terminal session
```bash
./server.py --batch 200 --validation --port 5556
```

3) Train steering model in a third terminal
```bash
./train_steering_model.py --port 5557 --val_port 5556
```

You may also choose to run the ./train.py convenience script to do all three at once.