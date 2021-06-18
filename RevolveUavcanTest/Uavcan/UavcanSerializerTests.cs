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
        }
    }
}
