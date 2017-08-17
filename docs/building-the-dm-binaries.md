# Building The Device Management Binaries

The following binaries constitute the Device Management solution:

- A UWP-compatible library to be included in the host UWP app (IoTDMClientLib).
- CommProxy.exe
- SystemConfigurator.exe

Below are the steps to build these components and have them ready for inclusion in your UWP application, and integrated into your device image.

### Enlist

  - Create a new folder, let's say `c:\iot-core-azure-dm-client`. and make it the current folder.
  - Run `git clone --recursive --branch master https://github.com/ms-iot/iot-core-azure-dm-client`

### Build The Binaries

  - Open `src\IoTDM.sln` in Visual Studio.
  - Set the configuration to *Release*.
  - Build the solution for each of the three architectures (ARM, x64, and x86).

### Build The Nuget Package

  - Open a Visual Studio command prompt and 
    - `cd c:\iot-core-azure-dm-client\nuget`
    - Run `PackIoTDMClientLib.cmd`

This will create `c:\iot-core-azure-dm-client\nuget\IoTDMClientLib.1.0.0.nupkg`.

**Next Step**:

- See the [DM Hello World! Application](dm-hello-world-overview.md).

----

[Home Page](../README.md)