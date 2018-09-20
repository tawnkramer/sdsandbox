from setuptools import setup

setup(name="donkey_gym",
      version="0.1",
      url="https://github.com/tawnkramer/sdsandbox/src/donkey_gym",
      author="Tawn Kramer",
      license="MIT",
      packages=["donkey_gym", "donkey_gym.envs"],
      package_data = {
          "donkey_gym.envs": ["samples/*.npy"]
      },
      #install_requires = ["gym", "numpy", 'flask', 'eventlet', 'socketio', 'pillow']
      install_requires = []
      )