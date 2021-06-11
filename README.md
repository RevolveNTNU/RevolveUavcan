# RevolveUavcan
Revolve NTNU's .NET Uavcan stack

# Goal
See if we can alter our UAVCAN stack to not be dependent on other Revolve Analyze internals, and create a NuGet package. May go open-source if achieved.

# TODO:

- Remove all Analyze internal references
- Find a reasonable output format for the parser
- Create a "main" function that can link parsers and framestorage etc.
- Define a minimalistic public API for the uavcan parser
- TESTING
- Write tests
- Setup CI (with henrik I hope)
