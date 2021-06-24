using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using RevolveUavcan.Communication.DataPackets;
using RevolveUavcan.Dsdl;
using RevolveUavcan.Dsdl.Interfaces;
using RevolveUavcan.Uavcan;
using System.Collections.Generic;

namespace RevolveUavcanTest.Uavcan
{
    [TestClass]
    public class UavcanParserTests
    {
        private Mock<UavcanSerializationRulesGenerator> mockRulesGenerator;
        private Mock<ILogger> loggerMock;
        private UavcanFrameStorage frameStorage;

        [TestInitialize]
        public void Setup()
        {
            mockRulesGenerator = new Mock<UavcanSerializationRulesGenerator>(new Mock<IDsdlParser>(""));
            loggerMock = new Mock<ILogger>();
            frameStorage = new UavcanFrameStorage(loggerMock.Object);
        }


        [DataTestMethod]
        [DynamicData(nameof(GetUavcanFrames), DynamicDataSourceType.Method)]
        public void ParseUavcanFrameTest(UavcanFrame frame)
        {
            var parser = new UavcanParser(loggerMock.Object, mockRulesGenerator.Object, frameStorage);

            var packets = new List<UavcanDataPacket>();

            parser.UavcanMessageParsed += delegate (object sender, UavcanDataPacket dataPacket)
            {
                packets.Add(dataPacket);
            };

            parser.ParseUavcanFrame(null, frame);

            Assert.IsTrue(packets.Count == 1);
        }

        public static IEnumerable<object[]> GetUavcanFrames()
        {
            yield return new object[] { };
        }
    }
}
