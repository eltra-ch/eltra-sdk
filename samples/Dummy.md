## Dummy sample

Dummy sample contains two console applications. Dummy Master and Agent.

### Dummy Master

To test the dummy process you have to start Dummy master first.

Dummy master represents the sample device that exposes it's interface over object dictionary file (xdd)

Dummy master has own name, product picture, version (xdd - DeviceIdentity node) and 3 parameters.

- PARAM_ControlWord (0x6040, 0x00) UINT16
- PARAM_StatusWord (0x6041, 0x00) UINT16
- PARAM_Counter (0x3000, 0x00) INT32

Additionally the master can handle 2 methods:

- StartCounting (parameters - step and delay, increase the PARAM_Counter with step value and wait specified dalay in miliseconds)
- StopCounting

Our sample device will use own credentials (sign-in call). With this credentials you can check the device activity on eltra.ch page.

It's recommended to use own credentials, to avoid the conflict with already registered device.

Optionally, you can create custom alias. With alias you can connect to the device, the same way as with credentials used to sign-in.
The advantage ist, that you can keep the original credentials secret and give the other party only alias data. 

Start the application and check the activity, the service should be registered and running.

### Dummy Agent

Dummy Agent is the remote party operator application. With this tool you can monitor the activity, actual parameter values and execute commands on device.

- Sign-in with your credentials (this is operator identity)
- Connect with device credentials (or alias)
- Retrieve active working channels
- Read devices information, execute commands, observe changing parameter "PARAM_Counter"