# Private API

## Send requests:

- ### Verify client (mandatory to execute and receive messages from the server)
    verify the client by sending the private key to the server
    ```json
    {
        "msg_type": "verify",
        "private_key": "XXXXXXXX"
    }
    ```

- ### Set random seed
    Set and update the seed used for generation, sending this with the right private key will also re-init the challenges.
    ```json
    {
        "msg_type": "set_random_seed",
        "seed":"42", 
    }
    ```

## Race Events:

- ### Collision with starting line
    event where a car hit a starting line
    ```json
    {
        "msg_type": "collision_with_starting_line",
        "car_name": "car_name",
        "starting_line_index": 0,
        "timeStamp": 0.0,
    }
    ```
    
- ### Collision with cone
    event where a car hit a cone
    ```json
    {
        "msg_type": "collision_with_cone",
        "car_name": "car_name",
        "cone_index": 0,
        "timeStamp": 0.0,
    }
    ```