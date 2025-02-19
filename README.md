![Build & Test](https://github.com/eltra-ch/eltra-sdk/workflows/Build%20&%20Test/badge.svg)

# ELTRA - CANOpen inspired IoT framework

ELTRA IoT framework is based on standard Web Protocols like REST, JSON and WebSocket.

**ELTRA SDK** cross-platform component is implemented and available in **.NET 8.0/9.0**.  

The main target was to create *secure, universal, multiplatform, lightweight, object-oriented and Industry 4.0 ready* solution that allows monitoring and control of any device in internet.

The concept is based on personal author experiance and inspired by technologies like *OPC UA, CANopen and FDT*.

CANopen concepts like object dictionary and/or publish-subscribe patterns can be found in this library.

This makes this solution easy to implement in industrial networks.

## Main features

- REST, WebSocket, UDP communication layers,
- HTTP(S) as base protocol 
- JSON message format
- Object dictionary support (CANopen, ISO 15745, CIA311),
- SDO service data object pattern implementation (CANopen),
- PDO publish-subscribe pattern support (process data object CANopen concepts),
- RPC calls with custom user defined commands,
- UDP Raw socket communication in internal networks
- Auto-Reconnection

In comparison to another similiar frameworks like **OPC UA** this technology is based on standard JSON/REST protocols with optional WebSocket and UDP support. 
**Woopsa** or **SiLA2** has similiar approach, but ELTRA has imho more sophisticated agent oriented architecture (each device and end-user is an agent working indirect over **hub service mediator**)

## ELTRA IoT Hub service

Test hub service is available under https://eltra.ch

Swagger documentation is available under https://eltra.ch/docs

## Main advantages

Easy to implement, cross-platform, proxy and firewall friendly, zero configuration, easy multiplatform web, pc, mobile application integration

## Supported HW/SW platforms	

    - .NET 8.0/9.0 compliant platforms
	- Xamarin 4/5, MAUI, WPF
    - tested on Windows 10/11, Linux, x64, ARM32 (Raspberry PI 3 and higher), Android, iPhone

## Source code topology
   
   * Connector (framework classes, connector classes for end-user or master device)
   * Common (common libraries used by the frameork, like contract classes definitions)
   * Ui (Xamarin, MAUI, WPF client libraries to speed up client implementation)

## Main classes

### AgentConnector

- Sign-in (and/or Sign-up)
- Connect (to cloud service with master device authentication)
- GetChannels (fetch the list of active channels and attached to channel device nodes)
- Device object has following functionality:
    - Retrive device properties (name, version, serial number)
    - Object dictionary (CANopen complient xml and OO representation)
    - Parameters (set, get, asynchron updates, history data, user level controlled authorization)
    - Commands (implementation of RPC on master device node)
    - Tools (supported views declaration)
- Sign-out

### MasterConnector

- Sign-in (and/or Sign-up with master credentials)
- Implement MasterDeviceManager derived class with Device nodes
- Inject your device implementation into connector and you are done
- Start service and enjoy - your device is available around the world

Implementing simple master agent shoudn't take longer as 1h 

Check samples directory. There you will fine sample master and agent implementation.

## Authors

* **Dawid Sienkiewicz**

See also the list of [contributors](https://github.com/eltra-ch/eltra/contributors) who participated in this project.

## License

This project is licensed under the Apache-2.0 License - see the [LICENSE.md](LICENSE.md) file for details