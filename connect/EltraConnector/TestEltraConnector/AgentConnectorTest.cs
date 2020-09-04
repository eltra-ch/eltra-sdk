using EltraConnector.Agent;
using EltraCommon.Contracts.Users;
using Xunit;
using System.Threading.Tasks;
using System;
using EltraCommon.Contracts.Devices;
using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;

namespace TestEltraConnector
{
    public class AgentConnectorTest : IDisposable
    {
        private AgentConnector _connector;
        private static UserIdentity _identity;
        private AgentConnectorTestData _testData;

        public AgentConnectorTest()
        {
            //string host = "https://eltra.ch";
            string host = "http://localhost:5001";

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

            //Act
            var parameter = await deviceNode1.GetParameter(0x4000, 0x01) as XddParameter;
            var actualValue = parameter.ActualValue.Clone();

            var parameterValue1 = await parameter.ReadValue();

            bool result = parameter.GetValue(out byte val1);

            if(val1 < byte.MaxValue - 1)
            {
                val1 = (byte)(val1 + 1);
            }
            else
            {
                val1 = byte.MinValue;
            }

            bool setValueResult = parameter.SetValue(val1);

            bool writeResult = await parameter.Write();

            var parameterValue2 = await parameter.ReadValue();

            byte val2 = byte.MinValue;

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
        public async Task Parameters_DeviceNode1ShouldHaveOperationalStringParameter()
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(1);

            //Act
            var parameter = await deviceNode1.GetParameter(0x4000, 0x0A) as XddParameter;
            var actualValue = parameter.ActualValue.Clone();

            var parameterValue1 = await parameter.ReadValue();

            bool result = parameter.GetValue(out string val1);

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
