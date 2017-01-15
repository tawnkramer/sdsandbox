import multiprocessing as mp
import server
import train_steering_model


#first start training server
ts = mp.Process( name = 'training server', target=server.run_default_train_server)
ts.start()

#then start validation server
vs = mp.Process( name = 'validation server', target=server.run_default_validation_server)
vs.start()

#then start training
train_steering_model.run_default_training('../outputs/steering_model/highway')
