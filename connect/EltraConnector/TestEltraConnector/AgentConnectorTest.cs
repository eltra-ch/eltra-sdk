using EltraConnector.Agent;
using EltraCommon.Contracts.Users;
using Xunit;
using System.Threading.Tasks;
using System;
using EltraCommon.Contracts.Devices;
using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Contracts.Parameters;
using EltraCommon.Contracts.CommandSets;
using System.Linq;

namespace TestEltraConnector
{
    public class AgentConnectorTest : IDisposable
    {
        private AgentConnector _connector;
        private static UserIdentity _identity;
        private AgentConnectorTestData _testData;

        public AgentConnectorTest()
        {
            string host = "https://eltra.ch";
            //string host = "http://localhost:5001";

            _connector = new AgentConnector() { Host = host };            
        }

        private AgentConnectorTestData TestData
        {
            get => _testData ?? (_testData = new AgentConnectorTestData(_connector, Identity));
        }

        private static UserIdentity Identity
        {
            get => _identity ?? (_identity = CreateUserIdentity());
        }

        private static UserIdentity CreateUserIdentity()
        {
            return new UserIdentity()
            {
                Login = Guid.NewGuid().ToString(),
                Password = "123456",
                Name = "Unit test user",
                Role = "developer"
            }; 
        }

        [Fact]
        public async Task Authentication_SignInWithAccountCreationShouldSucceed()
        {
            //Arrange
            
            //Act
            var result = await _connector.SignIn(Identity, true);

            //Assert
            Assert.True(result);            
        }

        [Fact]
        public async Task Authentication_SignInWithoutAccountCreationShouldFail()
        {
            //Arrange

            //Act
            var result = await _connector.SignIn(CreateUserIdentity());

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Authentication_ConnectToTestMasterShouldSucceed()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(Identity, true);

            //Act
            var result = await _connector.Connect();

            //Assert
            Assert.True(result, "Connect failed.");
            Assert.True(signInResult, "Sign-in failed.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Authentication_ConnectAndBindToTestMasterShouldSucceed()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(Identity, true);
            
            var deviceIdentity = new UserIdentity() { Login = "test.master@eltra.ch", Password = "1234" };

            //Act
            var result = await _connector.Connect(deviceIdentity);

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(result, "Connect failed.");
            
            await _connector.SignOut();
        }

        [Fact]
        public async Task Authentication_ConnectAndBindToTestMasterUsingAliasShouldSucceed()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(Identity, true);

            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };

            //Act
            var result = await _connector.Connect(deviceIdentity);

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(result, "Connect failed.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Authentication_SignOutShouldSucceed()
        {
            //Arrange
            await _connector.SignIn(Identity, true);

            //Act
            var result = await _connector.SignOut();

            //Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Channels_GetChannelsUsingAliasShouldSucceed()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(Identity, true);
            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };
            var connectResult = await _connector.Connect(deviceIdentity);

            //Act
            var channels = await _connector.GetChannels();

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");
            Assert.True(channels.Count > 0, "Get channels failed.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Channels_GetChannelLocationCountryCodeUsingAliasShouldSucceed()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(Identity, true);
            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };
            var connectResult = await _connector.Connect(deviceIdentity);
            var channels = await _connector.GetChannels();

            //Act
            string countryCode = string.Empty;
            foreach(var channel in channels)
            {
                countryCode = channel.Location.CountryCode;
            }

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");
            Assert.True(channels.Count > 0, "Get channels failed.");
            Assert.False(string.IsNullOrEmpty(countryCode), "Get country code failed.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Channels_GetChannelOwnerUserNameUsingAliasShouldSucceed()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(Identity, true);
            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };
            var connectResult = await _connector.Connect(deviceIdentity);
            var channels = await _connector.GetChannels();

            //Act
            string channelOwnerUserName = string.Empty;
            foreach (var channel in channels)
            {
                channelOwnerUserName = channel.UserName;
            }

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");
            Assert.True(channels.Count > 0, "Get channels failed.");
            Assert.False(string.IsNullOrEmpty(channelOwnerUserName), "Get owner user name failed.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Channels_ChannelShouldHaveDevices()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(Identity, true);
            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };
            var connectResult = await _connector.Connect(deviceIdentity);
            var channels = await _connector.GetChannels();
            int devicesCount = 0;

            foreach (var channel in channels)
            {
                devicesCount += channel.Devices.Count;
            }

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");
            Assert.True(channels.Count > 0, "Get channels failed.");
            Assert.True(devicesCount > 0, "Get devices failed.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Channels_ChannelShouldHaveDeviceWithNodeId1()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(Identity, true);
            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };
            var connectResult = await _connector.Connect(deviceIdentity);
            var channels = await _connector.GetChannels();
            EltraDevice foundDevice = null;

            foreach (var channel in channels)
            {
                foreach(var device in channel.Devices)
                {
                    if(device.NodeId == 1)
                    {
                        foundDevice = device;
                        break;
                    }
                }
            }

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");
            Assert.True(channels.Count > 0, "Get channels failed.");
            Assert.True(foundDevice != null, "Device with nodeid 1 not found.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Channels_ChannelShouldHaveDeviceWithNodeId2()
        {
            //Act
            var device = TestData.GetDevice(2);

            //Assert
            Assert.True(device != null, "Device with nodeid 2 not found.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Channels_ChannelShouldHaveDeviceWithNodeId3()
        {
            //Arrange
            var device = TestData.GetDevice(3);

            //Assert
            Assert.True(device != null, "Device with nodeid 3 not found.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Devices_DeviceNode1ShouldHaveIdentification()
        {
            //Arrange
            var device = await TestData.GetDevice(1);
            bool result = false;

            if (device != null)
            {
                if (!string.IsNullOrEmpty(device.Family))
                {
                    if (!string.IsNullOrEmpty(device.Name))
                    {
                        result = device.Identification.SerialNumber > 0;
                    }
                }
            }

            //Assert
            Assert.True(result, "Device identification missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Devices_DeviceNode2ShouldHaveIdentification()
        {
            //Arrange
            var device = await TestData.GetDevice(2);
            bool result = false;

            if (device != null)
            {
                if (!string.IsNullOrEmpty(device.Family))
                {
                    if (!string.IsNullOrEmpty(device.Name))
                    {
                        result = device.Identification.SerialNumber > 0;
                    }
                }
            }

            //Assert
            Assert.True(result, "Device identification missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Devices_DeviceNode3ShouldHaveIdentification()
        {
            //Arrange
            var device = await TestData.GetDevice(3);
            bool result = false;

            if (device != null)
            {
                if (!string.IsNullOrEmpty(device.Family))
                {
                    if (!string.IsNullOrEmpty(device.Name))
                    {
                        result = device.Identification.SerialNumber > 0;
                    }
                }
            }

            //Assert
            Assert.True(result, "Device identification missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Devices_DeviceNode1ShouldHaveObjectDictionary()
        {
            //Arrange
            var device = await TestData.GetDevice(1);

            //Act
            bool result = device.ObjectDictionary != null;

            //Assert
            Assert.True(result, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Devices_DeviceNode2ShouldHaveObjectDictionary()
        {
            //Arrange
            var device = await TestData.GetDevice(2);

            //Act
            bool result = device.ObjectDictionary != null;

            //Assert
            Assert.True(result, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Devices_DeviceNode3ShouldHaveObjectDictionary()
        {
            //Arrange
            var device = await TestData.GetDevice(3);

            //Act
            bool result = device.ObjectDictionary != null;

            //Assert
            Assert.True(result, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode1ObjectDictionaryShouldHaveCounterParameter()
        {
            //Arrange
            var device = await TestData.GetDevice(1);

            //Act
            var parameter = device.SearchParameter(0x3000, 0x00);

            //Assert
            Assert.True(parameter != null, "Device counter parameter missing missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode2ObjectDictionaryShouldHaveCounterParameter()
        {
            //Arrange
            var device = await TestData.GetDevice(2);

            //Act
            var parameter = device.SearchParameter(0x3000, 0x00);

            //Assert
            Assert.True(parameter != null, "Device counter parameter missing missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode3ObjectDictionaryShouldHaveCounterParameter()
        {
            //Arrange
            var device = await TestData.GetDevice(3);

            //Act
            var parameter = device.SearchParameter(0x3000, 0x00);

            //Assert
            Assert.True(parameter != null, "Device counter parameter missing missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode1ObjectDictionaryShouldHaveCounterParameterByUniqueId()
        {
            //Arrange
            var device = await TestData.GetDevice(1);

            //Act
            var parameter = device.SearchParameter("PARAM_Counter");

            //Assert
            Assert.True(parameter != null, "Device counter parameter missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode2ObjectDictionaryShouldHaveCounterParameterByUniqueId()
        {
            //Arrange
            var device = await TestData.GetDevice(2);

            //Act
            var parameter = device.SearchParameter("PARAM_Counter");

            //Assert
            Assert.True(parameter != null, "Device counter parameter missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode3ObjectDictionaryShouldHaveCounterParameterByUniqueId()
        {
            //Arrange
            var device = await TestData.GetDevice(2);

            //Act
            var parameter = device.SearchParameter("PARAM_Counter");

            //Assert
            Assert.True(parameter != null, "Device counter parameter missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode1ShouldHaveCounterParameter()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(1);

            //Act
            var parameter = deviceNode1.GetParameter(0x3000, 0x00);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode2ShouldHaveCounterParameter()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(2);

            //Act
            var parameter = deviceNode1.GetParameter(0x3000, 0x00);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode3ShouldHaveCounterParameter()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(3);

            //Act
            var parameter = deviceNode1.GetParameter(0x3000, 0x00);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode1ShouldHaveCounterParameterByUniqueId()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(1);

            //Act
            var parameter = deviceNode1.GetParameter("PARAM_Counter");

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode2ShouldHaveCounterParameterByUniqueId()
        {
            //Arrange
            var deviceNode2 = await TestData.GetDevice(2);

            //Act
            var parameter = deviceNode2.GetParameter("PARAM_Counter");

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode3ShouldHaveCounterParameterByUniqueId()
        {
            //Arrange
            var deviceNode2 = await TestData.GetDevice(3);

            //Act
            var parameter = deviceNode2.GetParameter("PARAM_Counter");

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode1ShouldHaveOperationalByteParameter()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(1);

            //Get parameter
            var parameter = await deviceNode1.GetParameter(0x4000, 0x01) as XddParameter;
            //store actual value for later use
            var actualValue = parameter.ActualValue.Clone();
            //update value, GetParameter is more 'heavy', so should be used once, and then only ReadValue
            var parameterValue1 = await parameter.ReadValue();
            //get value from local object dictionary
            bool result = parameter.GetValue(out byte val1);

            if(val1 < byte.MaxValue - 1)
            {
                val1 = (byte)(val1 + 1);
            }
            else
            {
                val1 = byte.MinValue;
            }

            // set new parameter value in object dictionary
            bool setValueResult = parameter.SetValue(val1);

            // push the data to remote device
            bool writeResult = await parameter.Write();

            // read current value from device
            var parameterValue2 = await parameter.ReadValue();

            byte val2 = byte.MinValue;

            // parameterValue2 should contain current device value
            bool getValueResult = parameterValue2.GetValue(ref val2);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");
            Assert.True(parameterValue1 != null, "Get ParameterValue failed.");
            Assert.True(parameterValue1.Equals(actualValue), "Get ParameterValue differs from actual value.");
            Assert.True(result, "GetValue failed.");
            Assert.True(parameterValue2 != null, "Get ParameterValue failed.");
            Assert.True(setValueResult, "SetValue failed.");
            Assert.True(getValueResult, "GetValue failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            Assert.True(writeResult, "Write failed.");

            // sign out
            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode1ShouldHaveOperationalSByteParameter()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(1);
            //Get parameter
            var parameter = await deviceNode1.GetParameter(0x4000, 0x05) as XddParameter;
            //store actual value for later use
            var actualValue = parameter.ActualValue.Clone();
            //update value, GetParameter is more 'heavy', so should be used once, and then only ReadValue
            var parameterValue1 = await parameter.ReadValue();
            //get value from local object dictionary
            bool result = parameter.GetValue(out sbyte val1);

            if (val1 < sbyte.MaxValue - 1)
            {
                val1 = (sbyte)(val1 + 1);
            }
            else
            {
                val1 = sbyte.MinValue;
            }

            // set new parameter value in object dictionary
            bool setValueResult = parameter.SetValue(val1);

            // push the data to remote device
            bool writeResult = await parameter.Write();

            // read current value from device
            var parameterValue2 = await parameter.ReadValue();

            sbyte val2 = sbyte.MinValue;

            // parameterValue2 should contain current device value
            bool getValueResult = parameterValue2.GetValue(ref val2);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");
            Assert.True(parameterValue1 != null, "Get ParameterValue failed.");
            Assert.True(parameterValue1.Equals(actualValue), "Get ParameterValue differs from actual value.");
            Assert.True(result, "GetValue failed.");
            Assert.True(parameterValue2 != null, "Get ParameterValue failed.");
            Assert.True(setValueResult, "SetValue failed.");
            Assert.True(getValueResult, "GetValue failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            Assert.True(writeResult, "Write failed.");

            // sign out
            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode1ShouldHaveOperationalUInt16Parameter()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(1);

            //Get parameter
            var parameter = await deviceNode1.GetParameter(0x4000, 0x02) as XddParameter;
            //store actual value for later use
            var actualValue = parameter.ActualValue.Clone();
            //update value, GetParameter is more 'heavy', so should be used once, and then only ReadValue
            var parameterValue1 = await parameter.ReadValue();
            //get value from local object dictionary
            bool result = parameter.GetValue(out ushort val1);

            if (val1 < ushort.MaxValue - 1)
            {
                val1 = (ushort)(val1 + 1);
            }
            else
            {
                val1 = ushort.MinValue;
            }

            // set new parameter value in object dictionary
            bool setValueResult = parameter.SetValue(val1);

            // push the data to remote device
            bool writeResult = await parameter.Write();

            // read current value from device
            var parameterValue2 = await parameter.ReadValue();

            ushort val2 = ushort.MinValue;

            // parameterValue2 should contain current device value
            bool getValueResult = parameterValue2.GetValue(ref val2);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");
            Assert.True(parameterValue1 != null, "Get ParameterValue failed.");
            Assert.True(parameterValue1.Equals(actualValue), "Get ParameterValue differs from actual value.");
            Assert.True(result, "GetValue failed.");
            Assert.True(parameterValue2 != null, "Get ParameterValue failed.");
            Assert.True(setValueResult, "SetValue failed.");
            Assert.True(getValueResult, "GetValue failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            Assert.True(writeResult, "Write failed.");

            // sign out
            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode1ShouldHaveOperationalUInt32Parameter()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(1);

            //Get parameter
            var parameter = await deviceNode1.GetParameter(0x4000, 0x03) as XddParameter;
            //store actual value for later use
            var actualValue = parameter.ActualValue.Clone();
            //update value, GetParameter is more 'heavy', so should be used once, and then only ReadValue
            var parameterValue1 = await parameter.ReadValue();
            //get value from local object dictionary
            bool result = parameter.GetValue(out uint val1);

            if (val1 < uint.MaxValue - 1)
            {
                val1 = (uint)(val1 + 1);
            }
            else
            {
                val1 = uint.MinValue;
            }

            // set new parameter value in object dictionary
            bool setValueResult = parameter.SetValue(val1);

            // push the data to remote device
            bool writeResult = await parameter.Write();

            // read current value from device
            var parameterValue2 = await parameter.ReadValue();

            uint val2 = uint.MinValue;

            // parameterValue2 should contain current device value
            bool getValueResult = parameterValue2.GetValue(ref val2);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");
            Assert.True(parameterValue1 != null, "Get ParameterValue failed.");
            Assert.True(parameterValue1.Equals(actualValue), "Get ParameterValue differs from actual value.");
            Assert.True(result, "GetValue failed.");
            Assert.True(parameterValue2 != null, "Get ParameterValue failed.");
            Assert.True(setValueResult, "SetValue failed.");
            Assert.True(getValueResult, "GetValue failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            Assert.True(writeResult, "Write failed.");

            // sign out
            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode1ShouldHaveOperationalUInt64Parameter()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(1);

            //Get parameter
            var parameter = await deviceNode1.GetParameter(0x4000, 0x04) as XddParameter;
            //store actual value for later use
            var actualValue = parameter.ActualValue.Clone();
            //update value, GetParameter is more 'heavy', so should be used once, and then only ReadValue
            var parameterValue1 = await parameter.ReadValue();
            //get value from local object dictionary
            bool result = parameter.GetValue(out ulong val1);

            if (val1 < ulong.MaxValue - 1)
            {
                val1 = (ulong)(val1 + 1);
            }
            else
            {
                val1 = ulong.MinValue;
            }

            // set new parameter value in object dictionary
            bool setValueResult = parameter.SetValue(val1);

            // push the data to remote device
            bool writeResult = await parameter.Write();

            // read current value from device
            var parameterValue2 = await parameter.ReadValue();

            ulong val2 = ulong.MinValue;

            // parameterValue2 should contain current device value
            bool getValueResult = parameterValue2.GetValue(ref val2);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");
            Assert.True(parameterValue1 != null, "Get ParameterValue failed.");
            Assert.True(parameterValue1.Equals(actualValue), "Get ParameterValue differs from actual value.");
            Assert.True(result, "GetValue failed.");
            Assert.True(parameterValue2 != null, "Get ParameterValue failed.");
            Assert.True(setValueResult, "SetValue failed.");
            Assert.True(getValueResult, "GetValue failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            Assert.True(writeResult, "Write failed.");

            // sign out
            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode1ShouldHaveOperationalInt16Parameter()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(1);

            //Get parameter
            var parameter = await deviceNode1.GetParameter(0x4000, 0x06) as XddParameter;
            //store actual value for later use
            var actualValue = parameter.ActualValue.Clone();
            //update value, GetParameter is more 'heavy', so should be used once, and then only ReadValue
            var parameterValue1 = await parameter.ReadValue();
            //get value from local object dictionary
            bool result = parameter.GetValue(out short val1);

            if (val1 < short.MaxValue - 1)
            {
                val1 = (short)(val1 + 1);
            }
            else
            {
                val1 = short.MinValue;
            }

            // set new parameter value in object dictionary
            bool setValueResult = parameter.SetValue(val1);

            // push the data to remote device
            bool writeResult = await parameter.Write();

            // read current value from device
            var parameterValue2 = await parameter.ReadValue();

            short val2 = short.MinValue;

            // parameterValue2 should contain current device value
            bool getValueResult = parameterValue2.GetValue(ref val2);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");
            Assert.True(parameterValue1 != null, "Get ParameterValue failed.");
            Assert.True(parameterValue1.Equals(actualValue), "Get ParameterValue differs from actual value.");
            Assert.True(result, "GetValue failed.");
            Assert.True(parameterValue2 != null, "Get ParameterValue failed.");
            Assert.True(setValueResult, "SetValue failed.");
            Assert.True(getValueResult, "GetValue failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            Assert.True(writeResult, "Write failed.");

            // sign out
            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode1ShouldHaveOperationalInt32Parameter()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(1);

            //Get parameter
            var parameter = await deviceNode1.GetParameter(0x4000, 0x07) as XddParameter;
            //store actual value for later use
            var actualValue = parameter.ActualValue.Clone();
            //update value, GetParameter is more 'heavy', so should be used once, and then only ReadValue
            var parameterValue1 = await parameter.ReadValue();
            //get value from local object dictionary
            bool result = parameter.GetValue(out int val1);

            if (val1 < int.MaxValue - 1)
            {
                val1 = (int)(val1 + 1);
            }
            else
            {
                val1 = int.MinValue;
            }

            // set new parameter value in object dictionary
            bool setValueResult = parameter.SetValue(val1);

            // push the data to remote device
            bool writeResult = await parameter.Write();

            // read current value from device
            var parameterValue2 = await parameter.ReadValue();

            int val2 = int.MinValue;

            // parameterValue2 should contain current device value
            bool getValueResult = parameterValue2.GetValue(ref val2);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");
            Assert.True(parameterValue1 != null, "Get ParameterValue failed.");
            Assert.True(parameterValue1.Equals(actualValue), "Get ParameterValue differs from actual value.");
            Assert.True(result, "GetValue failed.");
            Assert.True(parameterValue2 != null, "Get ParameterValue failed.");
            Assert.True(setValueResult, "SetValue failed.");
            Assert.True(getValueResult, "GetValue failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            Assert.True(writeResult, "Write failed.");

            // sign out
            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode1ShouldHaveOperationalInt64Parameter()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(1);

            //Get parameter
            var parameter = await deviceNode1.GetParameter(0x4000, 0x08) as XddParameter;
            //store actual value for later use
            var actualValue = parameter.ActualValue.Clone();
            //update value, GetParameter is more 'heavy', so should be used once, and then only ReadValue
            var parameterValue1 = await parameter.ReadValue();
            //get value from local object dictionary
            bool result = parameter.GetValue(out long val1);

            if (val1 < long.MaxValue - 1)
            {
                val1 = (long)(val1 + 1);
            }
            else
            {
                val1 = long.MinValue;
            }

            // set new parameter value in object dictionary
            bool setValueResult = parameter.SetValue(val1);

            // push the data to remote device
            bool writeResult = await parameter.Write();

            // read current value from device
            var parameterValue2 = await parameter.ReadValue();

            long val2 = long.MinValue;

            // parameterValue2 should contain current device value
            bool getValueResult = parameterValue2.GetValue(ref val2);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");
            Assert.True(parameterValue1 != null, "Get ParameterValue failed.");
            Assert.True(parameterValue1.Equals(actualValue), "Get ParameterValue differs from actual value.");
            Assert.True(result, "GetValue failed.");
            Assert.True(parameterValue2 != null, "Get ParameterValue failed.");
            Assert.True(setValueResult, "SetValue failed.");
            Assert.True(getValueResult, "GetValue failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            Assert.True(writeResult, "Write failed.");

            // sign out
            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode1ShouldHaveOperationalDoubleParameter()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(1);

            //Get parameter
            var parameter = await deviceNode1.GetParameter(0x4000, 0x09) as XddParameter;
            //store actual value for later use
            var actualValue = parameter.ActualValue.Clone();
            //update value, GetParameter is more 'heavy', so should be used once, and then only ReadValue
            var parameterValue1 = await parameter.ReadValue();
            //get value from local object dictionary
            bool result = parameter.GetValue(out double val1);

            if (val1 < double.MaxValue - 1)
            {
                val1 = (double)(val1 + 1);
            }
            else
            {
                val1 = double.MinValue;
            }

            // set new parameter value in object dictionary
            bool setValueResult = parameter.SetValue(val1);

            // push the data to remote device
            bool writeResult = await parameter.Write();

            // read current value from device
            var parameterValue2 = await parameter.ReadValue();

            double val2 = double.MinValue;

            // parameterValue2 should contain current device value
            bool getValueResult = parameterValue2.GetValue(ref val2);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");
            Assert.True(parameterValue1 != null, "Get ParameterValue failed.");
            Assert.True(parameterValue1.Equals(actualValue), "Get ParameterValue differs from actual value.");
            Assert.True(result, "GetValue failed.");
            Assert.True(parameterValue2 != null, "Get ParameterValue failed.");
            Assert.True(setValueResult, "SetValue failed.");
            Assert.True(getValueResult, "GetValue failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            Assert.True(writeResult, "Write failed.");

            // sign out
            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode1ShouldHaveOperationalStringParameter()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(1);

            //Act
            var parameter = await deviceNode1.GetParameter(0x4000, 0x0A) as XddParameter;
            var actualValue = parameter.ActualValue.Clone();

            var parameterValue1 = await parameter.ReadValue();

            bool result = parameter.GetValue(out string val1);

            if (string.IsNullOrEmpty(val1) || val1 == "Olleh")
            {
                val1 = "Hello";
            }
            else
            {
                val1 = "Olleh";
            }

            bool setValueResult = parameter.SetValue(val1);

            bool writeResult = await parameter.Write();

            var parameterValue2 = await parameter.ReadValue();

            string val2 = string.Empty;

            bool getValueResult = parameterValue2.GetValue(ref val2);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");
            Assert.True(parameterValue1 != null, "Get ParameterValue failed.");
            Assert.True(parameterValue1.Equals(actualValue), "Get ParameterValue differs from actual value.");
            Assert.True(result, "GetValue failed.");
            Assert.True(parameterValue2 != null, "Get ParameterValue failed.");
            Assert.True(setValueResult, "SetValue failed.");
            Assert.True(getValueResult, "GetValue failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            Assert.True(writeResult, "Write failed.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode1ShouldHaveOperationalDateTimeParameter()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(1);

            //Act
            var parameter = await deviceNode1.GetParameter(0x4000, 0x0C) as XddParameter;
            var actualValue = parameter.ActualValue.Clone();

            var parameterValue1 = await parameter.ReadValue();

            bool result = parameter.GetValue(out DateTime val0);

            var val1 = DateTime.Now;

            bool setValueResult = parameter.SetValue(val1);

            bool writeResult = await parameter.Write();

            var parameterValue2 = await parameter.ReadValue();

            DateTime val2 = DateTime.MinValue;

            bool getValueResult = parameterValue2.GetValue(ref val2);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");
            Assert.True(parameterValue1 != null, "Get ParameterValue failed.");
            Assert.True(parameterValue1.Equals(actualValue), "Get ParameterValue differs from actual value.");
            Assert.True(result, "GetValue failed.");
            Assert.True(parameterValue2 != null, "Get ParameterValue failed.");
            Assert.True(setValueResult, "SetValue failed.");
            Assert.True(getValueResult, "GetValue failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            Assert.True(writeResult, "Write failed.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode1ShouldHaveOperationalBooleanParameter()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(1);

            //Act
            var parameter = await deviceNode1.GetParameter(0x4000, 0x0D) as XddParameter;
            var actualValue = parameter.ActualValue.Clone();

            var parameterValue1 = await parameter.ReadValue();

            bool result = parameter.GetValue(out bool val1);

            val1 = !val1;

            bool setValueResult = parameter.SetValue(val1);

            bool writeResult = await parameter.Write();

            var parameterValue2 = await parameter.ReadValue();

            bool val2 = false;

            bool getValueResult = parameterValue2.GetValue(ref val2);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");
            Assert.True(parameterValue1 != null, "Get ParameterValue failed.");
            Assert.True(parameterValue1.Equals(actualValue), "Get ParameterValue differs from actual value.");
            Assert.True(result, "GetValue failed.");
            Assert.True(parameterValue2 != null, "Get ParameterValue failed.");
            Assert.True(setValueResult, "SetValue failed.");
            Assert.True(getValueResult, "GetValue failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            Assert.True(writeResult, "Write failed.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode1ShouldHaveOperationalIdentityParameter()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(1);

            //Get parameter
            var parameter = await deviceNode1.GetParameter(0x4000, 0x0E) as XddParameter;
            //store actual value for later use
            var actualValue = parameter.ActualValue.Clone();
            //update value, GetParameter is more 'heavy', so should be used once, and then only ReadValue
            var parameterValue1 = await parameter.ReadValue();
            //get value from local object dictionary
            bool result = parameter.GetValue(out byte val1);

            if (val1 < byte.MaxValue - 1)
            {
                val1 = (byte)(val1 + 1);
            }
            else
            {
                val1 = byte.MinValue;
            }

            // set new parameter value in object dictionary
            bool setValueResult = parameter.SetValue(val1);

            // push the data to remote device
            bool writeResult = await parameter.Write();

            // read current value from device
            var parameterValue2 = await parameter.ReadValue();

            byte val2 = byte.MinValue;

            // parameterValue2 should contain current device value
            bool getValueResult = parameterValue2.GetValue(ref val2);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");
            Assert.True(parameterValue1 != null, "Get ParameterValue failed.");
            Assert.True(parameterValue1.Equals(actualValue), "Get ParameterValue differs from actual value.");
            Assert.True(result, "GetValue failed.");
            Assert.True(parameterValue2 != null, "Get ParameterValue failed.");
            Assert.True(setValueResult, "SetValue failed.");
            Assert.True(getValueResult, "GetValue failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            Assert.True(writeResult, "Write failed.");

            // sign out
            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceNode1ShouldHaveOperationalObjectParameter()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(1);

            //Act
            var parameter = await deviceNode1.GetParameter(0x4000, 0x0B) as XddParameter;
            var actualValue = parameter.ActualValue.Clone();

            var parameterValue1 = await parameter.ReadValue();

            bool result = parameter.GetValue(out byte[] val0);

            byte[] val1 = new byte[128];

            for(byte a = 0; a < 128; a++)
            {
                val1[a] = a;
            }

            bool setValueResult = parameter.SetValue(val1);

            bool writeResult = await parameter.Write();

            var parameterValue2 = await parameter.ReadValue();

            var val2 = new byte[128];

            bool getValueResult = parameterValue2.GetValue(ref val2);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");
            Assert.True(parameterValue1 != null, "Get ParameterValue failed.");
            Assert.True(parameterValue1.Equals(actualValue), "Get ParameterValue differs from actual value.");
            Assert.True(result, "GetValue failed.");
            Assert.True(parameterValue2 != null, "Get ParameterValue failed.");
            Assert.True(setValueResult, "SetValue failed.");
            Assert.True(getValueResult, "GetValue failed.");
            Assert.True(val1.SequenceEqual(val2), $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            Assert.True(writeResult, "Write failed.");

            await _connector.SignOut();
        }

        [Fact(Skip = "major issues - not solved yet")]
        public async Task Parameters_DeviceNode1CountingParameterShouldAutoUpdate()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(1);

            //Act
            var parameter = await deviceNode1.GetParameter(0x3000, 0x00) as XddParameter;
            var initialValue = parameter.ActualValue.Clone();
            bool parameterChanged = false;
            //the parameter 0x3000 will be updated automatically 
            parameter.AutoUpdate(true, ParameterUpdatePriority.High, true);
            
            //let's observe the parameter changed event
            parameter.ParameterChanged += (sender, e) => {
            
                if(!e.NewValue.Equals(initialValue))
                {
                    parameterChanged = true;
                }
            };

            //let's execute start counting method on device
            var startCounting = await deviceNode1.GetCommand("StartCounting");
            
            startCounting.SetParameterValue<int>("Step", 1); //increase the value by step
            startCounting.SetParameterValue<int>("Delay", 10); //delay in ms between each step

            //execute start counting command, this should increase the 0x3000, 0x00 parameter every 10 ms by 1
            var startCountingResult = await startCounting.Execute();

            Assert.True(startCountingResult != null && startCountingResult.Status == ExecCommandStatus.Executed, "exec start counting failed!");

            //let's give him some time to respond
            await Task.Delay(1000);

            //Assert
            Assert.True(parameter != null, "Device parameter missing.");
            Assert.True(parameterChanged, "Parameter didn't change.");

            //call stop counting
            var stopCounting = await deviceNode1.GetCommand("StopCounting");

            var stopCountingResult = await stopCounting.Execute();

            parameter.AutoUpdate(false, ParameterUpdatePriority.High, true);

            await _connector.SignOut();
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool release)
        {
            //Cleanup
            var t = Task.Run(async ()=>
            {
                await _connector.SignOff();
            });

            t.Wait();

            _connector.Dispose();
        }
    }
}
