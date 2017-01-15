# Steering Angle model
Follow the instructions to train a deep neural network for self-steering cars.
This experiment is similar to [End to End Learning for Self-Driving
Cars](https://arxiv.org/abs/1604.07316).

1) 

2) Start training data server in the first terminal session
```bash
./server.py --batch 200 --port 5557
```  

3) Start validation data server in a second terminal session
```bash
./server.py --batch 200 --validation --port 5556
```

4) Train steering model in a third terminal
```bash
./train_steering_model.py --port 5557 --val_port 5556
```

5) 