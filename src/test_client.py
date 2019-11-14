import os
import time
import socket
import select
from threading import *
import sys
import select

host_ip = "127.0.0.1"
port = 9090
num_clients = 2
sockets = []
pause_on_create = 0.1
read_sockets = True
threads = []

def recv_msg(sock):
    sock.setblocking(0)
    inputs = [ sock ]
    outputs = []

    while read_sockets:
        try:
            readable, writable, exceptional = select.select(inputs, outputs, inputs)
            for s in readable:
                data, addr = s.recvfrom(1024 * 32)
                print(s.getsockname(), data.decode("utf-8"))
        except Exception as e:
            print(e)

for i in range(0, num_clients):
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

    # connecting to the server 
    s.connect((host_ip, port))

    th = Thread(target=recv_msg, args=(s,))
    threads.append(th)
    th.start()
    sockets.append(s)
    time.sleep(pause_on_create)
    if i == 0:
        s.send('{ "msg_type" : "load_scene", "scene_name" : "generated_road" }'.encode("utf-8"))

time.sleep(1)

print("waiting for read threads to stop")
read_sockets = False
for th in threads:
    th.join()

print("read threads to stopped")

sockets[0].send('{ "msg_type" : "exit_scene" }'.encode("utf-8"))

for s in sockets:
    s.close()
    time.sleep(0.1)

