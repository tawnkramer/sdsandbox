'''
file: donkey_proc.py
author: Felix Yu
date: 2018-09-12
'''

class DonkeyUnityProcess(object):

    def __init__(self):
        self.proc1 = None

    ## ------ Launch Unity Env ----------- ##

    def start(self, file_name, headless=False):

        launch_string = filename

        # Launch Unity environment
        if headless:
            self.proc1 = subprocess.Popen(
                [launch_string,'-nographics', '-batchmode'])
        else:
            self.proc1 = subprocess.Popen(
                [launch_string])

    def quit(self):
        """
        Shutdown unity environment
        """
        if self.proc1 is not None:
            self.proc1.kill()
    