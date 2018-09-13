'''
file: donkey_proc.py
author: Felix Yu
date: 2018-09-12
'''
import subprocess

class DonkeyUnityProcess(object):

    def __init__(self):
        self.proc1 = None

    ## ------ Launch Unity Env ----------- ##

    def start(self, sim_path, headless=False):

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
    