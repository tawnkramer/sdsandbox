import os
import time
import socket
import select
from threading import *
import sys
import select
import random
import json



class SDClient:
    def __init__(self, host, port):
        self.msg = None
        self.host = host
        self.port = port

        # the aborted flag will be set when we have detected a problem with the socket
        # that we can't recover from.
        self.aborted = False
        self.connect()


    def connect(self):
        self.s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

        # connecting to the server 
        self.s.connect((self.host, self.port))
        print("socket connected")

        # time.sleep(pause_on_create)
        self.do_process_msgs = True
        self.th = Thread(target=self.proc_msg, args=(self.s,))
        self.th.start()        


    def send(self, m):
        self.msg = m


    def on_msg_recv(self, j):
        # print("got:", j['msg_type'])
        # we will always have a 'msg_type' and will always get a json obj
        pass


    def reconnect(self):
        try:
            print("attempt reconnect")
            self.stop()
            self.connect()
        except Exception as e:
            print(e)
            return False
        return True


    def stop(self):
        self.do_process_msgs = False
        self.th.join()
        self.s.close()


    def proc_msg(self, sock):
        '''
        This is the thread message loop to process messages.
        We will send any message that is queued via the self.msg variable
        when our socket is in a writable state. 
        And we will read any messages when it's in a readable state and then
        call self.on_msg_recv with the json object message.
        '''
        #sock.setblocking(0)
        inputs = [ sock ]
        outputs = [ sock ]
        partial = []

        while self.do_process_msgs:
            time.sleep(0.05)
            data = "none"
            m = "none"
            try:
                readable, writable, exceptional = select.select(inputs, outputs, inputs)
                for s in readable:
                    # print("waiting to recv")
                    data = s.recv(1024 * 64)
                    
                    # we don't technically need to convert from bytes to string
                    # for json.loads, but we do need a string in order to do
                    # the split by \n newline char. This seperates each json msg.
                    data = data.decode("utf-8")
                    msgs = data.split("\n")

                    for m in msgs:
                        if len(m) < 2:
                            continue
                        last_char = m[-1]
                        first_char = m[0]
                        # check first and last char for a valid json terminator
                        # if not, then add to our partial packets list and see
                        # if we get the rest of the packet on our next go around.                
                        if first_char == "{" and last_char == '}':
                            j = json.loads(m)
                            self.on_msg_recv(j)
                        else:
                            partial.append(m)
                            if last_char == '}':
                                if partial[0][0] == "{":
                                    assembled_packet = "".join(partial)
                                    j = json.loads(assembled_packet)
                                    self.on_msg_recv(j)
                                else:
                                    print("failed packet.")
                                partial.clear()                            
                        
                for s in writable:
                    if self.msg != None:
                        # print("sending", self.msg)
                        s.sendall(self.msg.encode("utf-8"))
                        self.msg = None
                if len(exceptional) > 0:
                    print("problems w sockets!")
            except WindowsError as e:
                print("Exception:", e)
                if e.winerror == 10053:
                    # for some reason windows drops our connection.
                    self.aborted = True
                break
            except Exception as e:
                print("Exception:", e)
                print("Data:", m)
                self.aborted = True
                break



###########################################

def test_clients():
    # test params
    host_ip = "127.0.0.1"
    port = 9090
    num_clients = 1
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

    # Send driving controls
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
                # reconnect will 'work' but loses all vehicle state.
                # we could have something to repair this connection..
                # c.reconnect():
                print("Client problem, stopping driving.")
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

