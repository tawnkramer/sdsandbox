'''
file: donkey_proc.py
author: Felix Yu
date: 2018-09-12
'''
import subprocess
import os

class DonkeyUnityProcess(object):

    def __init__(self):
        self.proc1 = None

    ## ------ Launch Unity Env ----------- ##

    def start(self, sim_path, headless=False, port=9090):

        if not os.path.exists(sim_path):
            print(sim_path, "does not exist. not starting sim.")
            return

        port_args = ["--port", str(port), '-logFile', 'unitylog.txt']

        # Launch Unity environment
        if headless:
            self.proc1 = subprocess.Popen(
                [sim_path,'-nographics', '-batchmode'] + port_args)
        else:
            self.proc1 = subprocess.Popen(
                [sim_path] + port_args)

    def quit(self):
        """
        Shutdown unity environment
        """
        if self.proc1 is not None:
            print("closing donkey sim subprocess")
            self.proc1.kill()
    