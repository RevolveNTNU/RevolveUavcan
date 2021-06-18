using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolveUavcan.Dsdl.Fields;
using RevolveUavcan.Uavcan;
using System.Collections.Generic;

namespace RevolveUavcanTest.Uavcan
{
    [TestClass]
    public class UavcanSerializerTests
    {
        [DataTestMethod]
        [DynamicData(nameof(GetUavcanChannelsAndValues), DynamicDataSourceType.Method)]
        public void SerializeUavcanDataTest(UavcanFrame frame, List<UavcanChannel> uavcanChannels, List<double> channelValues, byte[] expectedData)
        {
            var serializedFrame = UavcanSerializer.SerializeUavcanData(uavcanChannels, channelValues, frame);

            CollectionAssert.AreEqual(serializedFrame.Data, expectedData);
        }

        public static IEnumerable<object[]> GetUavcanChannelsAndValues()
        {
            // Pitot tube message, simple message
            yield return new object[] {
                new UavcanFrame(),
                new List<UavcanChannel>
                {
                    new UavcanChannel(RevolveUavcan.Dsdl.Types.BaseType.FLOAT, 32, "pressure_delta"),
                    new UavcanChannel(RevolveUavcan.Dsdl.Types.BaseType.SIGNED_INT, 8, "error_bf")
                },
                new List<double> { 128.64, 1 },
                new byte[] { 215, 163, 0, 67, 1 }
            };

            // Message with each base type, with "abnormal" lenghts
            // int3 a
            // uint3 b
            // float32 c
            // float64 d
            // void16 e
            // bool f
            yield return new object[] {
                new UavcanFrame(),
                new List<UavcanChannel>
                {
                    new UavcanChannel(RevolveUavcan.Dsdl.Types.BaseType.SIGNED_INT, 3, "a"),
                    new UavcanChannel(RevolveUavcan.Dsdl.Types.BaseType.UNSIGNED_INT, 3, "b"),
                    new UavcanChannel(RevolveUavcan.Dsdl.Types.BaseType.FLOAT, 32, "c"),
                    new UavcanChannel(RevolveUavcan.Dsdl.Types.BaseType.FLOAT, 64, "d"),
                    new UavcanChannel(RevolveUavcan.Dsdl.Types.BaseType.VOID, 16, "e"),
                    new UavcanChannel(RevolveUavcan.Dsdl.Types.BaseType.BOOLEAN, 1, "f")
                },
                new List<double> { -1, 1, 128.64, 256.18, 1 },
                new byte[] { 207, 245, 40, 192, 208, 30, 133, 235, 81, 184, 0, 28, 16, 0, 64 }
            };
        }
    }
}
