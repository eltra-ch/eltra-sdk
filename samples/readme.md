## Dummy sample

Dummy sample contains two console applications. Dummy Master and Agent.

Start Dummy Master to instantiate the dummy device in system.

Start Dummy Agent to monitor and control dummy master in your console application.

### UnitTests

UnitTests contains the unit tests master for connector agent. 

This process is ussed to test the SDK on eltra.ch web service.

### Maxon - EPOS4

Maxon EPOS4 master sample - first 'real life' example - EPOS4 master is able to scan and register your over USB, CAN or RS232 connected EPOS4 controller to ELTRA.

Writing the agent that controls such device is as easy, as using SDK and using built-in commands like MoveToPosition, MoveWithVelocity and many more. 
Monitoring processing CANOpen objects is realised as usuall (GetParameter/GetParameterValue/AutoUpdate). 

 