import os
import random
import json
import time
from io import BytesIO
import base64

from PIL import Image
import numpy as np
from gym_donkeycar.core.sim_client import SDClient


###########################################

class SimpleClient(SDClient):

    def __init__(self, address, poll_socket_sleep_time=0.01):
        super().__init__(*address, poll_socket_sleep_time=poll_socket_sleep_time)
        self.last_image = None
        self.car_loaded = False

    def on_msg_recv(self, json_packet):

        if json_packet['msg_type'] == "car_loaded":
            self.car_loaded = True
        
        if json_packet['msg_type'] == "telemetry":
            imgString = json_packet["image"]
            image = Image.open(BytesIO(base64.b64decode(imgString)))
            self.last_image = np.asarray(image)

            #don't have to, but to clean up the print, delete the image string.
            del json_packet["image"]

        print("got:", json_packet)


    def send_controls(self, steering, throttle):
        p = { "msg_type" : "control",
                "steering" : steering.__str__(),
                "throttle" : throttle.__str__(),
                "brake" : "0.0" }
        msg = json.dumps(p)
        self.send(msg)

        #this sleep lets the SDClient thread poll our message and send it out.
        time.sleep(self.poll_socket_sleep_sec)

    def update(self):
        # just random steering now
        st = random.random() * 2.0 - 1.0
        th = 0.3
        self.send_controls(st, th)



###########################################
## Make some clients and have them connect with the simulator

def test_clients():
    # test params
    host = "127.0.0.1" # "trainmydonkey.com" for virtual racing server
    port = 9091
    num_clients = 1
    clients = []
    time_to_drive = 10.0


    # Start Clients
    for _ in range(0, num_clients):
        c = SimpleClient(address=(host, port))
        clients.append(c)

    time.sleep(1)

    # Load Scene message. Only one client needs to send the load scene.
    msg = '{ "msg_type" : "load_scene", "scene_name" : "generated_track" }'
    clients[0].send(msg)

    # Wait briefly for the scene to load.
    loaded = False
    while(not loaded):
        time.sleep(1.0)
        for c in clients:
            loaded = c.car_loaded           
        

    # Car config
    msg = '{ "msg_type" : "car_config", "body_style" : "donkey", "body_r" : "255", "body_g" : "0", "body_b" : "255", "car_name" : "Tawn", "font_size" : "100" }'
    clients[0].send(msg)
    time.sleep(1)


    # Send random driving controls
    start = time.time()
    do_drive = True
    while time.time() - start < time_to_drive and do_drive:
        for c in clients:
            c.update()
            if c.aborted:
                print("Client socket problem, stopping driving.")
                do_drive = False

    time.sleep(1.0)

    # Exist Scene
    msg = '{ "msg_type" : "exit_scene" }'
    clients[0].send(msg)

    time.sleep(1.0)

    # Close down clients
    print("waiting for msg loop to stop")
    for c in clients:
        c.stop()

    print("clients to stopped")



if __name__ == "__main__":
    test_clients()

