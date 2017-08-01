from __future__ import print_function
import math

def mag_vec3(v):
    '''
    take a tuple of 3 components, 
    return the magnitide
    '''
    return math.sqrt(v[0] ** 2 + v[1] ** 2 + v[2] ** 2)


class ThrottleManager(object):
    '''
    manage the speed of a car
    compute throttle and brake values given the current
    velocity and steering
    '''
    def __init__(self, idealSpeed = 10.0, 
                        turnSlowFactor = 3.0,
                        brakeThresh = 100.0,
                        constThrottleReq = 0.5 ):
        #the ideal speed sets the target meters per second speed
        self.idealSpeed = idealSpeed

        #slow factor defines a scalar that affects how much we slow when turning
        self.turnSlowFactor = turnSlowFactor
        
        #the brakeThresh defines the speed at which we apply brakes
        self.brakeThresh = brakeThresh

        #if we are below target speed, this is the throttle applied to increase speed
        self.constThrottleReq = constThrottleReq


    def get_throttle_brake(self, car_vel_mag, car_steering):
        '''
        take the car velocity and steering
        returns a throttle and brake value
        '''
        speedFactor = car_vel_mag * math.fabs(car_steering)

        idealSpeedAdjusted = self.idealSpeed - (self.turnSlowFactor * math.fabs(car_steering))

        #print('idealSpeedAdjusted', idealSpeedAdjusted, "car_steering", car_steering)

        if(speedFactor > self.brakeThresh):
            return 0.0, 1.0
        elif(car_vel_mag < idealSpeedAdjusted):
            return self.constThrottleReq, 0.0
        else:
            return 0.01, 0.0
    