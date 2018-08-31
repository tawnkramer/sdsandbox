from gym.envs.registration import register


register(
    id='donkey-generated-roads-v0',
    entry_point='donkey_gym.envs:GeneratedRoadsEnv',
    timestep_limit=2000,
)

register(
    id='donkey-warehouse-v0',
    entry_point='donkey_gym.envs:WarehouseEnv',
    timestep_limit=2000,
)

register(
    id='donkey-avc-sparkfun-v0',
    entry_point='donkey_gym.envs:AvcSparkfunEnv',
    timestep_limit=2000,
)

