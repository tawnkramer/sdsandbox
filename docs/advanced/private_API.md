# Private API


Set and update the seed used for generation, sending this with the right private key will also re-init the challenges.
```json
{
    "msg_type": "set_random_seed",
    "private_key":"00000000",
    "seed":"0", 
}
```