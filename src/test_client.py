import os
import random
import json
import time

from gym_donkeycar.core.sim_client import SDClient

###########################################

def test_clients():
    # test params
    host_ip = "127.0.0.1" # "trainmydonkey.com" for virtual racing server
    port = 9091
    num_clients = 2
    clients = []
    time_to_drive = 40.0


    # Start Clients
    for _ in range(0, num_clients):
        c = SDClient(host_ip, port)
        clients.append(c)

    time.sleep(1)

    # Load Scene message. Only one client needs to send the load scene.
    msg = '{ "msg_type" : "load_scene", "scene_name" : "generated_track" }'
    clients[0].send(msg)

    # Wait briefly for the scene to load.
    time.sleep(1.0)

    # Car config
    msg = '{ "msg_type" : "car_config", "body_style" : "donkey", "body_r" : "255", "body_g" : "0", "body_b" : "255", "car_name" : "Tawn", "font_size" : "100" }'
    clients[0].send(msg)
    time.sleep(1)

    msg = '{ "msg_type" : "car_config", "body_style" : "car01", "body_r" : "0", "body_g" : "0", "body_b" : "255", "car_name" : "Doug" }'
    if num_clients > 1:
        clients[1].send(msg)
    time.sleep(1)


    # Send random driving controls
    start = time.time()
    do_drive = True
    while time.time() - start < time_to_drive and do_drive:
        for c in clients:
            st = random.random() * 2.0 - 1.0
            th = 0.3
            p = { "msg_type" : "control",
                "steering" : st.__str__(),
                "throttle" : th.__str__(),
                "brake" : "0.0" }
            msg = json.dumps(p)
            c.send(msg)
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

