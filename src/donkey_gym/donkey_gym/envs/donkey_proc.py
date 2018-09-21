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

    def start(self, sim_path, headless=False):

        if not os.path.exists(sim_path):
            print(sim_path, "does not exist. not starting sim.")
            return

        # Launch Unity environment
        if headless:
            self.proc1 = subprocess.Popen(
                [sim_path,'-nographics', '-batchmode'])
        else:
            self.proc1 = subprocess.Popen(
                [sim_path])

    def quit(self):
        """
        Shutdown unity environment
        """
        if self.proc1 is not None:
            self.proc1.kill()
    