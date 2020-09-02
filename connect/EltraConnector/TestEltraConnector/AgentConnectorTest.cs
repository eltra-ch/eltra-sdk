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
        private UserIdentity _identity;

        public AgentConnectorTest()
        {
            string host = "https://eltra.ch";
            //string host = "http://localhost:5001";

            _connector = new AgentConnector() { Host = host };

            _identity = new UserIdentity()
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
            var result = await _connector.SignIn(_identity, true);

            //Assert
            Assert.True(result);            
        }

        [Fact]
        public async Task Authentication_SignInWithoutAccountCreationShouldFail()
        {
            //Arrange

            //Act
            var result = await _connector.SignIn(_identity);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Authentication_ConnectToTestMasterShouldSucceed()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(_identity, true);

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
            bool signInResult = await _connector.SignIn(_identity, true);
            
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
            bool signInResult = await _connector.SignIn(_identity, true);

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
            await _connector.SignIn(_identity, true);

            //Act
            var result = await _connector.SignOut();

            //Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Channels_GetChannelsUsingAliasShouldSucceed()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(_identity, true);
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
            bool signInResult = await _connector.SignIn(_identity, true);
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
            bool signInResult = await _connector.SignIn(_identity, true);
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
            bool signInResult = await _connector.SignIn(_identity, true);
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
            bool signInResult = await _connector.SignIn(_identity, true);
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
            //Arrange
            bool signInResult = await _connector.SignIn(_identity, true);
            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };
            var connectResult = await _connector.Connect(deviceIdentity);
            var channels = await _connector.GetChannels();
            EltraDevice foundDevice = null;

            foreach (var channel in channels)
            {
                foreach (var device in channel.Devices)
                {
                    if (device.NodeId == 2)
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
            Assert.True(foundDevice != null, "Device with nodeid 2 not found.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Channels_ChannelShouldHaveDeviceWithNodeId3()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(_identity, true);
            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };
            var connectResult = await _connector.Connect(deviceIdentity);
            var channels = await _connector.GetChannels();
            EltraDevice foundDevice = null;

            foreach (var channel in channels)
            {
                foreach (var device in channel.Devices)
                {
                    if (device.NodeId == 3)
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
            Assert.True(foundDevice != null, "Device with nodeid 3 not found.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Devices_DeviceShouldHaveIdentification()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(_identity, true);
            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };
            var connectResult = await _connector.Connect(deviceIdentity);
            var channels = await _connector.GetChannels();
            bool result = false;

            foreach (var channel in channels)
            {
                foreach(var device in channel.Devices)
                {
                    if(!string.IsNullOrEmpty(device.Family))
                    {
                        if (!string.IsNullOrEmpty(device.Name))
                        {
                            result = device.Identification.SerialNumber > 0;
                        }
                    }                    
                }
            }

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");
            Assert.True(channels.Count > 0, "Get channels failed.");
            Assert.True(result, "Device identification missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Devices_DeviceShouldHaveObjectDictionary()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(_identity, true);
            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };
            var connectResult = await _connector.Connect(deviceIdentity);
            var channels = await _connector.GetChannels();
            bool result = false;

            foreach (var channel in channels)
            {
                foreach (var device in channel.Devices)
                {
                    result = device.ObjectDictionary != null;
                }
            }

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");
            Assert.True(channels.Count > 0, "Get channels failed.");
            Assert.True(result, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceObjectDictionaryShouldHaveCounterParameter()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(_identity, true);
            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };
            var connectResult = await _connector.Connect(deviceIdentity);
            var channels = await _connector.GetChannels();
            EltraDevice foundDevice = null;

            foreach (var channel in channels)
            {
                foreach (var device in channel.Devices)
                {
                    foundDevice = device;
                    break;
                }
            }

            //Act
            var parameter = foundDevice.SearchParameter(0x3000, 0x00);

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");
            Assert.True(channels.Count > 0, "Get channels failed.");
            Assert.True(parameter != null, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceObjectDictionaryShouldHaveCounterParameterByUniqueId()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(_identity, true);
            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };
            var connectResult = await _connector.Connect(deviceIdentity);
            var channels = await _connector.GetChannels();
            EltraDevice foundDevice = null;

            foreach (var channel in channels)
            {
                foreach (var device in channel.Devices)
                {
                    foundDevice = device;
                    break;
                }
            }

            //Act
            var parameter = foundDevice.SearchParameter("PARAM_Counter");

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");
            Assert.True(channels.Count > 0, "Get channels failed.");
            Assert.True(parameter != null, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceShouldHaveCounterParameter()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(_identity, true);
            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };
            var connectResult = await _connector.Connect(deviceIdentity);
            var channels = await _connector.GetChannels();
            EltraDevice foundDevice = null;

            foreach (var channel in channels)
            {
                foreach (var device in channel.Devices)
                {
                    foundDevice = device;
                    break;
                }
            }

            //Act
            var parameter = foundDevice.GetParameter(0x3000, 0x00);

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");
            Assert.True(channels.Count > 0, "Get channels failed.");
            Assert.True(parameter != null, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceShouldHaveCounterParameterByUniqueId()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(_identity, true);
            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };
            var connectResult = await _connector.Connect(deviceIdentity);
            var channels = await _connector.GetChannels();
            EltraDevice foundDevice = null;

            foreach (var channel in channels)
            {
                foreach (var device in channel.Devices)
                {
                    foundDevice = device;
                    break;
                }
            }

            //Act
            var parameter = foundDevice.GetParameter("PARAM_Counter");

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");
            Assert.True(channels.Count > 0, "Get channels failed.");
            Assert.True(parameter != null, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Parameters_DeviceShouldHaveByteParameter()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(_identity, true);
            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };
            var connectResult = await _connector.Connect(deviceIdentity);
            var channels = await _connector.GetChannels();
            EltraDevice foundDevice = null;

            foreach (var channel in channels)
            {
                foreach (var device in channel.Devices)
                {
                    foundDevice = device;
                    break;
                }
            }

            //Act
            var parameter = await foundDevice.GetParameter(0x4000, 0x01) as XddParameter;

            var parameterValue = await parameter.ReadValue();

            bool result = parameter.GetValue(out byte val);

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");
            Assert.True(channels.Count > 0, "Get channels failed.");
            Assert.True(parameter != null, "Device object dictionary missing.");
            Assert.True(result, "GetValue failed.");
            Assert.True(parameterValue != null, "Get ParameterValue failed.");
            
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
