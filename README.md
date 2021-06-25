<div class="row">
  <div class="column">

  </div>
  <div class="column">

  </div>
</div>
<p float="left">
    <a href="https://revolve.no/"><img align=left src="https://raw.githubusercontent.com/RevolveNTNU/RevolveUavcan/tribe/.github/main/revolve.svg" width="30%"/>
    <a href="https://uavcan.org/"><img align=right margin src="https://raw.githubusercontent.com/RevolveNTNU/RevolveUavcan/tribe/.github/main/uavcan.svg" width="30%"/>
</p>
<br>
<br>
<br>
<br>
<br>

[![Build, run tests and generate report](https://github.com/RevolveNTNU/RevolveUavcan/actions/workflows/build_and_test.yml/badge.svg)](https://github.com/RevolveNTNU/RevolveUavcan/actions/workflows/build_and_test.yml)
[![Coverage Status](https://coveralls.io/repos/github/RevolveNTNU/RevolveUavcan/badge.svg?branch=tribe)](https://coveralls.io/github/RevolveNTNU/RevolveUavcan?branch=tribe)

# Table of contents

1. [Introduction](#Introduction)
1. [Userguide](#Userguide)
1. [Limitations](#Limitations)

# Introduction
Revolve NTNU's .NET Uavcan stack, used in our Telemetry and Analysis software Revolve Analyze. Includes a runtime dsdl interpeter and dynamic uavcan (de)serialization. This library does not configure your .NET project to be a live node on the UAVCAN Network. It only serves as a datalogger with the ability to write messages back. It includes a communication interface that has to be implemented in your communication module code.

The UAVCAN specification can be found here: [UAVCAN Spec](https://uavcan.org/specification/UAVCAN_Specification_v1.0-beta.pdf), this library currently supports version v1.0-beta.

# Userguide
There is a userguide available on the Github wiki, which is found here: [Wiki](https://github.com/RevolveNTNU/RevolveUavcan/wiki). Here, each component is presented, as well as a quick-start guide to get you started!

# Contributing
Feel free to contribute to this library! Setting up the development environment is very straight forward. Just clone the repository and open the solution in Visual Studio and you should be good to go! More details can be found in the CONTRIBUTING file.

Please open an issue when you start on a new feature or a bugfix, and merge via a Pull Request. The Tribe branch is the main branch.

Tests can be exectured using dotnet test, and are writing using the MSTest v2 framework. More on this in the [Wiki](https://github.com/RevolveNTNU/RevolveUavcan/wiki).

# Limitations
There are currently some limitations present in this implementation, some by choice, some not. These might get fixed as time passes.

- Does not consider @ commands in the DSDL
- Does not support serialization of Multiframe messages
- Multiframe reading does not work perfectly, suspect some threading issues
- Does not support dynamic array lengths
- All data is parsed as doubles, this is due to a design limitation in our Desktop Application Revolve Analyze.

