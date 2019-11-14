import os
import time
import socket
import select
from threading import *
import sys

host_ip = "127.0.0.1"
port = 9090
num_clients = 1
sockets = []
pause_on_create = 0.1

def recv_msg(sock):
    while True:
        try:
            data, addr = sock.recvfrom(1024)
            print(data.decode("utf-8"))
        except:
            break

for i in range(0, num_clients):
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

    # connecting to the server 
    s.connect((host_ip, port))

    Thread(target=recv_msg, args=(s,)).start()
    sockets.append(s)
    time.sleep(pause_on_create)
    s.send('{ "msg_type" : "load_scene", "scene_name" : "generated_road" }'.encode("utf-8"))

time.sleep(5)

for s in sockets:
    s.send('{ "msg_type" : "bye" }'.encode("utf-8"))
    s.close()
    time.sleep(1)

