# RevolveUavcan
Revolve NTNU's .NET Uavcan stack, used in our Telemetry and Analysis software Revolve Analyze. Includes a runtime dsdl interpeter and dynamic uavcan (de)serialization. 

# Limitations
There are currently some limitations present in this implementation, some by choice, some not. These might get fixed as time passes.

- Does not consider @ commands in the DSDL
- Does not support serialization of Multiframe messages
- Does only support CAN-FD message sizes, meaning that 64 is the max message size.
- Multiframe reading does not work perfectly, suspect some threading issues

# Test coverage
[![Coverage Status](https://coveralls.io/repos/github/RevolveNTNU/RevolveUavcan/badge.svg?branch=tribe)](https://coveralls.io/github/RevolveNTNU/RevolveUavcan?branch=tribe)
