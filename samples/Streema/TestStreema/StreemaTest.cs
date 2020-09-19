using Xunit;
using System;
using System.Threading.Tasks;
using EltraConnector.Agent;
using EltraCommon.Contracts.Users;
using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;

namespace TestStreema
{
    public class StreemaTest : IDisposable
    {
        private AgentConnector _connector;
        private static UserIdentity _identity;
        private StreemaTestData _testData;

        public StreemaTest()
        {
            //string host = "https://eltra.ch";
            string host = "http://localhost:5001";

            _connector = new AgentConnector() { Host = host };            
        }

        private StreemaTestData TestData
        {
            get => _testData ?? (_testData = new StreemaTestData(_connector, Identity));
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

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(1, 3)]
        [InlineData(1, 4)]
        [InlineData(1, 5)]
        public async Task Parameters_DeviceNodeObjectDictionaryShouldHaveUrlParameter(int nodeId, byte subIndex)
        {
            //Arrange
            var device = await TestData.GetDevice(nodeId);

            //Act
            var parameter = device.SearchParameter(0x4000, subIndex);

            //Assert
            Assert.True(parameter != null, $"Device counter parameter 0x{subIndex:X4} missing missing.");

            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(1, 3)]
        [InlineData(1, 4)]
        [InlineData(1, 5)]
        public async Task Parameters_DeviceNodeShouldHaveUrlParameter(int nodeId, byte subIndex)
        {
            //Arrange
            var deviceNode1 = await TestData.GetDevice(nodeId);

            //Act
            var parameter = deviceNode1.GetParameter(0x4000, subIndex);

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        public async Task Parameters_DeviceNodeShouldHaveActiveStationParameterByUniqueId(int nodeId)
        {
            //Arrange
            var deviceNode = await TestData.GetDevice(nodeId);

            //Act
            var parameter = deviceNode.GetParameter("PARAM_ActiveStation");

            //Assert
            Assert.True(parameter != null, "Device object dictionary missing.");

            await _connector.SignOut();
        }

        [Theory]
        [InlineData(1)]
        public async Task Parameters_DeviceNodeShouldHaveOperationalActiveStationParameter(int nodeId)
        {
            //Arrange
            var deviceNode = await TestData.GetDevice(nodeId);
            //Get parameter
            var parameter = await deviceNode.GetParameter(0x4001, 0x00) as XddParameter;
            //store actual value for later use
            var actualValue = parameter.ActualValue.Clone();
            //update value, GetParameter is more 'heavy', so should be used once, and then only ReadValue
            var parameterValue1 = await parameter.ReadValue();
            //get value from local object dictionary
            bool result = parameter.GetValue(out int val1);

            int minValue = 0;
            int maxValue = 0;
            bool rangeResult = parameter.GetRange(ref minValue, ref maxValue);

            if (val1 < maxValue)
            {
                val1 = (int)(val1 + 1);
            }
            else
            {
                val1 = minValue;
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
            Assert.True(rangeResult, "GetRange failed.");
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
        public async Task Parameters_DeviceNodeShouldHaveOperationalVolumeParameter(int nodeId)
        {
            //Arrange
            var deviceNode = await TestData.GetDevice(nodeId);
            //Get parameter
            var parameter = await deviceNode.GetParameter(0x4002, 0x00) as XddParameter;
            //store actual value for later use
            var actualValue = parameter.ActualValue.Clone();
            //update value, GetParameter is more 'heavy', so should be used once, and then only ReadValue
            var parameterValue1 = await parameter.ReadValue();
            //get value from local object dictionary
            bool result = parameter.GetValue(out int val1);

            int minValue = 0;
            int maxValue = 0;

            bool rangeResult = parameter.GetRange(ref minValue, ref maxValue);
            
            if (val1 < maxValue)
            {
                val1 = (int)(val1 + 1);
            }
            else
            {
                val1 = minValue;
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
            Assert.True(rangeResult, "GetRange failed.");
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
        public async Task Parameters_DeviceNodeShouldHaveOperationalControlWordParameter(int nodeId)
        {
            //Arrange
            var deviceNode = await TestData.GetDevice(nodeId);

            //Get parameter
            var parameter = await deviceNode.GetParameter(0x6040, 0x00) as XddParameter;
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
