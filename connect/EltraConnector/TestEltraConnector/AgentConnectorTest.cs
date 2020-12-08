using EltraConnector.Agent;
using EltraCommon.Contracts.Users;
using Xunit;
using System.Threading.Tasks;
using System;
using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Contracts.Parameters;
using EltraCommon.Contracts.CommandSets;
using System.Linq;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;

namespace TestEltraConnector
{
    public class AgentConnectorTest : IDisposable
    {
        //private string _host = "https://eltra.ch";
        private string _host = "http://localhost:5001";

        private AgentConnector _connector;
        private static UserIdentity _identity;
        private AgentConnectorTestData _testData;
        private string _aliasDeviceLogin = "test2@eltra.ch";
        private string _aliasDevicePassword = "1234";
        private string masterDeviceLogin = "test.master2@eltra.ch";

        public AgentConnectorTest()
        {
            _connector = new AgentConnector() { Host = _host };            
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
                Login = Guid.NewGuid().ToString() + "@eltra.ch",
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
        public async Task Authentication_ConnectConcurrently2ConnectorsToTestMasterShouldSucceed()
        {
            //Arrange
            bool signInResult1 = await _connector.SignIn(Identity, true);
            var secondConnector = new AgentConnector() { Host = _host };
            var secondIdentity = CreateUserIdentity();
            bool signInResult2 = await secondConnector.SignIn(secondIdentity, true);

            //Act
            var result1 = await _connector.Connect();

            var result2 = await secondConnector.Connect();

            //Assert
            Assert.True(signInResult1, "Sign-in first connector failed.");
            Assert.True(signInResult2, "Sign-in second connector failed.");
            Assert.True(result1, "Connect first connector failed.");
            Assert.True(result2, "Connect second connector failed.");
            
            await _connector.SignOut();
            await secondConnector.SignOut();
        }

        [Fact]
        public async Task Authentication_ConnectAndBindToTestMasterShouldSucceed()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(Identity, true);
            
            var deviceIdentity = new UserIdentity() { Login = masterDeviceLogin, Password = "1234" };

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

            var deviceIdentity = new UserIdentity() { Login = _aliasDeviceLogin, Password = _aliasDevicePassword };

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
        public async Task Connection_ConnectDisconnectShouldSucceed()
        {
            //Arrange
            await _connector.SignIn(Identity, true);

            //Act
            bool connectResult = await _connector.Connect();

            _connector.Disconnect();

            await _connector.SignOut();

            //Assert
            Assert.True(connectResult);
        }

        [Fact]
        public async Task Channels_GetChannelsUsingAliasShouldSucceed()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(Identity, true);
            var deviceIdentity = new UserIdentity() { Login = _aliasDeviceLogin, Password = _aliasDevicePassword };
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
            var deviceIdentity = new UserIdentity() { Login = _aliasDeviceLogin, Password = _aliasDevicePassword };
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
            var deviceIdentity = new UserIdentity() { Login = _aliasDeviceLogin, Password = _aliasDevicePassword };
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
            var deviceIdentity = new UserIdentity() { Login = _aliasDeviceLogin, Password = _aliasDevicePassword };
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
        public async Task Channels_ChannelCanBeBinded()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(Identity, true);
            var deviceIdentity = new UserIdentity() { Login = _aliasDeviceLogin, Password = _aliasDevicePassword };
            var connectResult = await _connector.Connect();
            
            //Act
            var bindResult = await _connector.BindChannels(deviceIdentity);
            
            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");
            Assert.True(bindResult, "Bind channels failed.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Channels_ChannelCanBeBindedAndUnbound()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(Identity, true);
            var deviceIdentity = new UserIdentity() { Login = _aliasDeviceLogin, Password = _aliasDevicePassword };
            var connectResult = await _connector.Connect();
            var bindResult = await _connector.BindChannels(deviceIdentity);

            Assert.True(bindResult, "Bind channels failed.");

            var channels = await _connector.GetChannels();

            Assert.True(channels.Count > 0, "Get channels failed (1).");

            foreach (var channel in channels)
            {
                var unbindResult = await _connector.UnbindChannel(channel);

                Assert.True(unbindResult, "Un-bind channels failed.");
            }

            channels = await _connector.GetChannels();

            Assert.True(channels.Count == 0, $"Binded channels remains count = {channels.Count}.");

            //Assert            
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");
            
            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Channels_ChannelShouldHaveDeviceWithNodeId(int nodeId)
        {
            //Arrange
            var device = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

            //Assert
            Assert.True(device != null, "Device with nodeid 3 not found.");

            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Devices_DeviceNodeShouldHaveIdentification(int nodeId)
        {
            //Arrange
            var device = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);
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

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Devices_DeviceNodeShouldHaveObjectDictionary(int nodeId)
        {
            //Arrange
            var device = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

            //Act
            bool result = device.ObjectDictionary != null;

            //Assert
            Assert.True(result, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeObjectDictionaryShouldHaveCounterParameter(int nodeId)
        {
            //Arrange
            var device = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

            //Act
            var parameter = device.SearchParameter(0x3000, 0x00);

            //Assert
            Assert.True(parameter != null, "Device counter parameter missing missing.");

            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeObjectDictionaryShouldHaveCounterParameterByUniqueId(int nodeId)
        {
            //Arrange
            var device = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

            //Act
            var parameter = device.SearchParameter("PARAM_Counter");

            //Assert
            Assert.True(parameter != null, "Device counter parameter missing.");

            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeShouldHaveCounterParameter(int nodeId)
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

            //Act
            var parameter = deviceNode1.GetParameter(0x3000, 0x00);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeShouldHaveCounterParameterByUniqueId(int nodeId)
        {
            //Arrange
            var deviceNode2 = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

            //Act
            var parameter = deviceNode2.GetParameter("PARAM_Counter");

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeShouldHaveOperationalByteParameter(int nodeId)
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

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
            Assert.True(writeResult, "Write failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            
            // sign out
            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeShouldHaveOperationalSByteParameter(int nodeId)
        {
            //Arrange
            var deviceNode = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);
            //Get parameter
            var parameter = await deviceNode.GetParameter(0x4000, 0x05) as XddParameter;
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
            Assert.True(writeResult, "Write failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            
            // sign out
            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeShouldHaveOperationalUInt16Parameter(int nodeId)
        {
            //Arrange
            var deviceNode = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

            //Get parameter
            var parameter = await deviceNode.GetParameter(0x4000, 0x02) as XddParameter;
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
            Assert.True(writeResult, "Write failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            
            // sign out
            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeShouldHaveOperationalUInt32Parameter(int nodeId)
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

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
            Assert.True(writeResult, "Write failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            
            // sign out
            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeShouldHaveOperationalUInt64Parameter(int nodeId)
        {
            //Arrange
            var deviceNode = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

            //Get parameter
            var parameter = await deviceNode.GetParameter(0x4000, 0x04) as XddParameter;
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
            Assert.True(writeResult, "Write failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            
            // sign out
            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeShouldHaveOperationalInt16Parameter(int nodeId)
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

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
            Assert.True(writeResult, "Write failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
           
            // sign out
            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeShouldHaveOperationalInt32Parameter(int nodeId)
        {
            //Arrange
            var deviceNode = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

            //Get parameter
            var parameter = await deviceNode.GetParameter(0x4000, 0x07) as XddParameter;
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
            Assert.True(writeResult, "Write failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            
            // sign out
            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeShouldHaveOperationalInt64Parameter(int nodeId)
        {
            //Arrange
            var deviceNode = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

            //Get parameter
            var parameter = await deviceNode.GetParameter(0x4000, 0x08) as XddParameter;
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
            Assert.True(writeResult, "Write failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            
            // sign out
            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeShouldHaveOperationalDoubleParameter(int nodeId)
        {
            //Arrange
            var deviceNode = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

            //Get parameter
            var parameter = await deviceNode.GetParameter(0x4000, 0x09) as XddParameter;
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
            Assert.True(writeResult, "Write failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            
            // sign out
            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeShouldHaveOperationalStringParameter(int nodeId)
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

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
            Assert.True(writeResult, "Write failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            
            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeShouldHaveOperationalDateTimeParameter(int nodeId)
        {
            //Arrange
            var deviceNode = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

            //Act
            var parameter = await deviceNode.GetParameter(0x4000, 0x0C) as XddParameter;
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
            Assert.True(writeResult, "Write failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            
            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeShouldHaveOperationalBooleanParameter(int nodeId)
        {
            //Arrange
            var deviceNode = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

            //Act
            var parameter = await deviceNode.GetParameter(0x4000, 0x0D) as XddParameter;
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
            Assert.True(writeResult, "Write failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            
            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeShouldHaveOperationalIdentityParameter(int nodeId)
        {
            //Arrange
            var deviceNode = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

            //Get parameter
            var parameter = await deviceNode.GetParameter(0x4000, 0x0E) as XddParameter;
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
            Assert.True(writeResult, "Write failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");
            
            // sign out
            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeShouldHaveOperationalObjectParameter(int nodeId)
        {
            //Arrange
            var deviceNode = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

            //Act
            var parameter = await deviceNode.GetParameter(0x4000, 0x0B) as XddParameter;
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

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Parameters_DeviceNodeCountingParameterShouldAutoUpdate(int nodeId)
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

            await Task.Delay(1000);

            //Act
            var parameter = await deviceNode1.GetParameter(0x3000, 0x00) as XddParameter;
            var initialValue = parameter.ActualValue.Clone();
            bool parameterChanged = false;
            //the parameter 0x3000 will be updated automatically 
            parameter.AutoUpdate(ParameterUpdatePriority.High, true);
            
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
            startCounting.SetParameterValue<int>("Delay", 100); //delay in ms between each step

            //execute start counting command, this should increase the 0x3000, 0x00 parameter every 'Delay' [ms] by 'Step'
            var startCountingResult = await startCounting.Execute();

            Assert.True(startCountingResult != null && startCountingResult.Status == ExecCommandStatus.Executed, "exec start counting failed!");

            //let's give him some time to respond
            await Task.Delay(500);

            //call stop counting
            var stopCounting = await deviceNode1.GetCommand("StopCounting");

            var stopCountingResult = await stopCounting.Execute();

            parameter.StopUpdate(ParameterUpdatePriority.High, true);

            //Assert
            Assert.True(parameter != null, "Device parameter missing.");
            Assert.True(parameterChanged, "Parameter didn't change.");

            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1, 50)]
        [InlineData(2, 50)]
        [InlineData(3, 50)]
        public async Task Parameters_DeviceNodeReadInt32PerformanceTest(int nodeId, int maxCount)
        {
            //Arrange
            var deviceNode = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

            int count = 0;
            XddParameter parameter;
            ParameterValue actualValue;
            ParameterValue parameterValue1;

            do
            {
                //Get parameter
                parameter = await deviceNode.GetParameter(0x4000, 0x07) as XddParameter;
                //store actual value for later use
                actualValue = parameter.ActualValue.Clone();
                //update value, GetParameter is more 'heavy', so should be used once, and then only ReadValue
                parameterValue1 = await parameter.ReadValue();

                count++;
            }
            while (parameter != null && actualValue != null && parameterValue1 != null && count < maxCount);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");
            Assert.True(parameterValue1 != null, "Get ParameterValue failed.");
            Assert.True(parameterValue1.Equals(actualValue), "Get ParameterValue differs from actual value.");

            // sign out
            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1, 50)]
        [InlineData(2, 50)]
        [InlineData(3, 50)]
        public async Task Parameters_DeviceNodeReadWriteInt32PerformanceTest(int nodeId, int maxCount)
        {
            //Arrange
            var deviceNode = await TestData.GetDevice(nodeId, _aliasDeviceLogin, _aliasDevicePassword);

            bool result = false;
            bool getValueResult = false;
            bool setValueResult = false;
            bool writeResult = false;
            int count = 0;
            XddParameter parameter;
            ParameterValue actualValue;
            ParameterValue parameterValue1;
            ParameterValue parameterValue2;
            int val1, val2;

            do
            {
                //Get parameter
                parameter = await deviceNode.GetParameter(0x4000, 0x07) as XddParameter;
                //store actual value for later use
                actualValue = parameter.ActualValue.Clone();
                //update value, GetParameter is more 'heavy', so should be used once, and then only ReadValue
                parameterValue1 = await parameter.ReadValue();
                //get value from local object dictionary
                result = parameter.GetValue(out val1);

                if (val1 < int.MaxValue - 1)
                {
                    val1 = (int)(val1 + 1);
                }
                else
                {
                    val1 = int.MinValue;
                }

                // set new parameter value in object dictionary
                setValueResult = parameter.SetValue(val1);

                // push the data to remote device
                writeResult = await parameter.Write();

                // read current value from device
                parameterValue2 = await parameter.ReadValue();

                val2 = int.MinValue;

                // parameterValue2 should contain current device value
                getValueResult = parameterValue2.GetValue(ref val2);

                count++;
            }
            while (result && getValueResult && writeResult && setValueResult && count < maxCount);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");
            Assert.True(parameterValue1 != null, "Get ParameterValue failed.");
            Assert.True(parameterValue1.Equals(actualValue), "Get ParameterValue differs from actual value.");
            Assert.True(parameterValue2 != null, "Get ParameterValue failed.");
            Assert.True(val1 == val2, $"ReadValue/Write mismatch val1 = {val1} val2 = {val2}.");

            Assert.True(result, "GetValue failed.");
            Assert.True(setValueResult, "SetValue failed.");
            Assert.True(getValueResult, "GetValue failed.");
            Assert.True(writeResult, "Write failed.");
            
            // sign out
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
