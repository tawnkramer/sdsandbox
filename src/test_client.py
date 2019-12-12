import os
import random
import json
import time

from gym_donkeycar.core.sim_client import SDClient

###########################################

def test_clients():
    # test params
    host_ip = "127.0.0.1"
    port = 9091
    num_clients = 4
    clients = []
    pause_on_create = 1.0
    time_to_drive = 20.0


    # Start Clients
    for _ in range(0, num_clients):
        c = SDClient(host_ip, port)
        clients.append(c)

    time.sleep(pause_on_create)

    # Load Scene
    msg = '{ "msg_type" : "load_scene", "scene_name" : "generated_road" }'
    clients[0].send(msg)
    time.sleep(1.0)

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

