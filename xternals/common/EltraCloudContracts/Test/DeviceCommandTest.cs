using System;
using EposCloudContracts.Contracts.CommandSets;
using Xunit;

namespace EposCloudContractsTest
{
    public class DeviceCommandTest
    {
        [Fact]
        public void SingleSetParameterValueShouldFail()
        {
            //arrange
            var command = new DeviceCommand();
            Single value = Single.Epsilon;

            //act
            var result = command.SetParameterValue("Index", value);

            //validate
            Assert.False(result);
        }

        [Fact]
        public void DecimalSetParameterValueShouldFail()
        {
            //arrange
            var command = new DeviceCommand();
            Decimal value = Decimal.MinusOne;

            //act
            var result = command.SetParameterValue("Index", value);

            //validate
            Assert.False(result);
        }

        [Fact]
        public void ByteSetParameterValueShouldSucceed()
        {
            //arrange
            var command = new DeviceCommand();

            //act
            var result = command.SetParameterValue("Index", (byte)0x10);

            //validate
            Assert.True(result);
        }

        [Fact]
        public void SByteSetParameterValueShouldSucceed()
        {
            //arrange
            var command = new DeviceCommand();

            //act
            var result = command.SetParameterValue("Index", (sbyte)0x10);

            //validate
            Assert.True(result);
        }

        [Fact]
        public void CharSetParameterValueShouldSucceed()
        {
            //arrange
            var command = new DeviceCommand();

            //act
            var result = command.SetParameterValue("Index", (char)0x10);

            //validate
            Assert.True(result);
        }

        [Fact]
        public void U16SetParameterValueShouldSucceed()
        {
            //arrange
            var command = new DeviceCommand();

            //act
            var result = command.SetParameterValue("Index", (ushort)0x1000);
            
            //validate
            Assert.True(result);
        }

        [Fact]
        public void U32SetParameterValueShouldSucceed()
        {
            //arrange
            var command = new DeviceCommand();

            //act
            var result = command.SetParameterValue("Index", (uint)0x10001000);

            //validate
            Assert.True(result);
        }

        [Fact]
        public void U64SetParameterValueShouldSucceed()
        {
            //arrange
            var command = new DeviceCommand();

            //act
            var result = command.SetParameterValue("Index", (UInt64)0x1000100010001000);

            //validate
            Assert.True(result);
        }

        [Fact]
        public void I16SetParameterValueShouldSucceed()
        {
            //arrange
            var command = new DeviceCommand();

            //act
            var result = command.SetParameterValue("Index", (short)-0x1000);

            //validate
            Assert.True(result);
        }

        [Fact]
        public void I32SetParameterValueShouldSucceed()
        {
            //arrange
            var command = new DeviceCommand();

            //act
            var result = command.SetParameterValue("Index", -0x10001000);

            //validate
            Assert.True(result);
        }

        [Fact]
        public void I64SetParameterValueShouldSucceed()
        {
            //arrange
            var command = new DeviceCommand();

            //act
            var result = command.SetParameterValue("Index", -0x1000100010001000);

            //validate
            Assert.True(result);
        }

        [Fact]
        public void ByteArraySetParameterValueShouldSucceed()
        {
            //arrange
            var command = new DeviceCommand();

            //act
            var result = command.SetParameterValue("Index", new byte[] { 0x1,0x2,0x3 });

            //validate
            Assert.True(result);
        }

        [Fact]
        public void StringSetParameterValueShouldSucceed()
        {
            //arrange
            var command = new DeviceCommand();

            //act
            var result = command.SetParameterValue("Index", "hello world");

            //validate
            Assert.True(result);
        }

        [Fact]
        public void BooleanSetParameterValueShouldSucceed()
        {
            //arrange
            var command = new DeviceCommand();

            //act
            var result = command.SetParameterValue("Index", true);

            //validate
            Assert.True(result);
        }

        [Fact]
        public void BooleanGetParameterValueShouldReturnSameValue()
        {
            //arrange
            var command = new DeviceCommand();
            const bool expected = true;
            bool result = false;

            command.SetParameterValue("Index", expected);

            command.GetParameterValue("Index", ref result);

            //validate
            Assert.Equal(result, expected);
        }

        [Fact]
        public void ByteGetParameterValueShouldReturnSameValue()
        {
            //arrange
            var command = new DeviceCommand();
            byte result = 0;
            byte expected = byte.MaxValue;

            command.SetParameterValue("Index", expected);

            //act
            command.GetParameterValue("Index", ref result);

            //validate
            Assert.Equal(result, expected);
        }

        [Fact]
        public void SByteGetParameterValueShouldReturnSameValue()
        {
            //arrange
            var command = new DeviceCommand();
            sbyte result = 0;
            sbyte expected = sbyte.MinValue;

            command.SetParameterValue("Index", expected);

            //act
            command.GetParameterValue("Index", ref result);

            //validate
            Assert.Equal(result, expected);
        }

        [Fact]
        public void CharGetParameterValueShouldReturnSameValue()
        {
            //arrange
            var command = new DeviceCommand();
            char result = char.MaxValue;
            char expected = char.MinValue;

            command.SetParameterValue("Index", expected);

            //act
            command.GetParameterValue("Index", ref result);

            //validate
            Assert.Equal(result, expected);
        }

        [Fact]
        public void U16GetParameterValueShouldReturnSameValue()
        {
            //arrange
            var command = new DeviceCommand();
            ushort result = 0;
            ushort expected = ushort.MaxValue;

            command.SetParameterValue("Index", expected);

            //act
            command.GetParameterValue("Index", ref result);

            //validate
            Assert.Equal(result, expected);
        }

        [Fact]
        public void U32GetParameterValueShouldReturnSameValue()
        {
            //arrange
            var command = new DeviceCommand();
            uint result = 0;
            uint expected = uint.MaxValue;

            command.SetParameterValue("Index", expected);

            //act
            command.GetParameterValue("Index", ref result);

            //validate
            Assert.Equal(result, expected);
        }

        [Fact]
        public void U64GetParameterValueShouldReturnSameValue()
        {
            //arrange
            var command = new DeviceCommand();
            ulong result = 0;
            ulong expected = ulong.MaxValue;

            command.SetParameterValue("Index", expected);

            //act
            command.GetParameterValue("Index", ref result);

            //validate
            Assert.Equal(result, expected);
        }

        [Fact]
        public void I16GetParameterValueShouldReturnSameValue()
        {
            //arrange
            var command = new DeviceCommand();
            short result = 0;
            short expected = short.MinValue;

            command.SetParameterValue("Index", expected);

            //act
            command.GetParameterValue("Index", ref result);

            //validate
            Assert.Equal(result, expected);
        }

        [Fact]
        public void I32GetParameterValueShouldReturnSameValue()
        {
            //arrange
            var command = new DeviceCommand();
            int result = 0;
            int expected = int.MinValue;

            command.SetParameterValue("Index", expected);

            //act
            command.GetParameterValue("Index", ref result);

            //validate
            Assert.Equal(result, expected);
        }

        [Fact]
        public void I64GetParameterValueShouldReturnSameValue()
        {
            //arrange
            var command = new DeviceCommand();
            long result = 0;
            long expected = long.MinValue;

            command.SetParameterValue("Index", expected);

            //act
            command.GetParameterValue("Index", ref result);

            //validate
            Assert.Equal(result, expected);
        }

        [Fact]
        public void ByteArrayGetParameterValueShouldReturnSameValue()
        {
            //arrange
            var command = new DeviceCommand();
            byte[] result = {};
            byte[] expected = { 0x1, 0x2, 0x3 };

            command.SetParameterValue("Index", expected);

            //act
            command.GetParameterValue("Index", ref result);

            //validate
            Assert.Equal(result, expected);
        }

        [Fact]
        public void StringGetParameterValueShouldReturnSameValue()
        {
            //arrange
            var command = new DeviceCommand();
            string result = string.Empty;
            string expected = "hello world";

            command.SetParameterValue("Index", expected);
            
            //act
            command.GetParameterValue("Index", ref result);
            
            //validate
            Assert.Equal(result, expected);
        }
    }
}
