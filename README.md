# Trapezoidal move profile for PWM (pulse width modulated) DC motors
Code for implementing a trapezoidal move profile for PWM driven motors, which results in a soft motion with ramp-up, movement, and ramp-down. This is particularly important in robotics where you want smooth motions. 

This code was written as part of a 6-DOF robotic arm project and has been tested on a real, physical robotic arm.

The algorithm is found in `TrapezoidalMoveProfile.cs`.

![image](https://user-images.githubusercontent.com/32486318/194780047-a78380dd-2d6c-4d33-b25c-919bcfba0927.png)
