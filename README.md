# RevolveUavcan
Revolve NTNU's .NET Uavcan stack, used in our Telemetry and Analysis software Revolve Analyze. Includes a runtime dsdl interpeter and dynamic uavcan (de)serialization. 

# Limitations
There are currently some limitations present in this implementation, some by choice, some not. These might get fixed as time passes.

- Does not consider @ commands in the DSDL
- Does not support serialization of Multiframe messages
- Does only support CAN-FD message sizes, meaning that 64 is the max message size.
- Multiframe reading does not work perfectly, suspect some threading issues

# Test coverage
[![codecov](https://codecov.io/gh/RevolveNTNU/RevolveUavcan/branch/tribe/graph/badge.svg?token=SAaMpwbrCT)](https://codecov.io/gh/RevolveNTNU/RevolveUavcan)
