using System;
using System.Linq;

namespace EposMaster.DeviceManager.Converters
{
    static class ObjectDictionaryConverter
    {
        public static byte[] ConvertAddressToByteArray(ushort index, byte subIndex)
        {
            var byteArray = new byte[4];

            byteArray[0] = 0;
            byteArray[1] = subIndex;
            byteArray[2] = BitConverter.GetBytes(index)[0];
            byteArray[3] = BitConverter.GetBytes(index)[1];

            return byteArray;
        }

        public static uint ConvertAddressToVariable(ushort index, byte subIndex)
        {
            var byteArray = ConvertAddressToByteArray(index, subIndex);

            return BitConverter.ToUInt32(byteArray,0);
        }

        public static void ConvertToAddress(byte[] byteArray, ref ushort index, ref byte subIndex)
        {
            if (byteArray.Length == 4)
            {
                index = BitConverter.ToUInt16(byteArray.Skip(2).Take(2).ToArray(),0);
                subIndex = byteArray[1];
            }
        }
    }
}
