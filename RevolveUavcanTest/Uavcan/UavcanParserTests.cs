using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RevolveUavcan.Communication.DataPackets;
using RevolveUavcan.Dsdl.Fields;
using RevolveUavcan.Uavcan;
using RevolveUavcan.Uavcan.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace RevolveUavcanTest.Uavcan
{
    [TestClass]
    public class UavcanParserTests
    {
        private Mock<IUavcanSerializationGenerator> rulesGeneratorMock;
        private Mock<ILogger<UavcanParser>> parserLoggerMock;
        private Mock<ILogger<UavcanFrameStorage>> frameStorageLoggerMock;
        private UavcanFrameStorage frameStorage;

        [TestInitialize]
        public void Setup()
        {
            rulesGeneratorMock = new Mock<IUavcanSerializationGenerator>();
            parserLoggerMock = new Mock<ILogger<UavcanParser>>();
            frameStorageLoggerMock = new Mock<ILogger<UavcanFrameStorage>>();
            frameStorage = new UavcanFrameStorage(frameStorageLoggerMock.Object);
        }


        [DataTestMethod]
        [DynamicData(nameof(GetUavcanFrames), DynamicDataSourceType.Method)]
        public void ParseUavcanFrameMessageTest(UavcanFrame frame, List<UavcanChannel> serializationRule, uint subjectId, List<double> values)
        {
            rulesGeneratorMock.Setup(s => s.TryGetSerializationRuleForMessage(subjectId, out serializationRule)).Returns(true);

            var parser = new UavcanParser(parserLoggerMock.Object, rulesGeneratorMock.Object, frameStorage);

            var packets = new List<UavcanDataPacket>();

            parser.UavcanMessageParsed += delegate (object sender, UavcanDataPacket dataPacket)
            {
                packets.Add(dataPacket);
            };

            parser.ParseUavcanFrame(null, frame);

            Assert.IsTrue(packets.Count == 1);

            var parsedDataDict = packets.First().ParsedDataDict;

            var valueIndex = 0;
            foreach (var channel in serializationRule.FindAll(x => x.Basetype != RevolveUavcan.Dsdl.Types.BaseType.VOID))
            {
                Assert.AreEqual(parsedDataDict[channel], values[valueIndex], 0.0001);
                valueIndex++;
            }
        }

        public static IEnumerable<object[]> GetUavcanFrames()
        {
            // Simple Pitot Tube message
            var serializationRules = new List<UavcanChannel> {
                    new UavcanChannel(RevolveUavcan.Dsdl.Types.BaseType.FLOAT, 32, "pressure_delta"),
                    new UavcanChannel(RevolveUavcan.Dsdl.Types.BaseType.SIGNED_INT, 8, "error_bf")
            };

            var values = new List<double> { 128.64, 1 };
            var data = new byte[] { 215, 163, 0, 67, 1 };
            uint subjectId = 413;

            yield return new object[] { new UavcanFrame() { Data = data, SubjectId = subjectId, IsServiceNotMessage = false }, serializationRules, subjectId, values };

            serializationRules = new List<UavcanChannel>{
                    new UavcanChannel(RevolveUavcan.Dsdl.Types.BaseType.SIGNED_INT, 3, "a"),
                    new UavcanChannel(RevolveUavcan.Dsdl.Types.BaseType.UNSIGNED_INT, 3, "b"),
                    new UavcanChannel(RevolveUavcan.Dsdl.Types.BaseType.FLOAT, 32, "c"),
                    new UavcanChannel(RevolveUavcan.Dsdl.Types.BaseType.FLOAT, 64, "d"),
                    new UavcanChannel(RevolveUavcan.Dsdl.Types.BaseType.VOID, 16, "e"),
                    new UavcanChannel(RevolveUavcan.Dsdl.Types.BaseType.BOOLEAN, 1, "f")
            };
            values = new List<double> { -1, 1, 128.64, 256.18, 1 };
            data = new byte[] { 207, 245, 40, 192, 208, 30, 133, 235, 81, 184, 0, 28, 16, 0, 64 };
            subjectId = 60;

            yield return new object[] { new UavcanFrame() { Data = data, SubjectId = subjectId, IsServiceNotMessage = false }, serializationRules, subjectId, values };
        }

        [DataTestMethod]
        [DynamicData(nameof(GetUavcanServiceFrames), DynamicDataSourceType.Method)]
        public void ParseUavcanFrameServiceTest(UavcanFrame frame, UavcanService service, List<UavcanChannel> serializationRule, uint subjectId, List<double> values)
        {
            rulesGeneratorMock.Setup(s => s.TryGetSerializationRuleForService(subjectId, out service)).Returns(true);

            var parser = new UavcanParser(parserLoggerMock.Object, rulesGeneratorMock.Object, frameStorage);

            var packets = new List<UavcanDataPacket>();

            parser.UavcanServiceParsed += delegate (object sender, UavcanDataPacket dataPacket)
            {
                packets.Add(dataPacket);
            };

            parser.ParseUavcanFrame(null, frame);

            Assert.IsTrue(packets.Count == 1);

            var parsedDataDict = packets.First().ParsedDataDict;

            var valueIndex = 0;
            foreach (var channel in serializationRule.FindAll(x => x.Basetype != RevolveUavcan.Dsdl.Types.BaseType.VOID))
            {
                Assert.AreEqual(parsedDataDict[channel], values[valueIndex], 0.0001);
                valueIndex++;
            }
        }

        public static IEnumerable<object[]> GetUavcanServiceFrames()
        {
            // Simple RTDS Service
            var serializationRequestRules = new List<UavcanChannel> {
                    new UavcanChannel(RevolveUavcan.Dsdl.Types.BaseType.SIGNED_INT, 8, "command")
            };

            var serializationResponseRules = new List<UavcanChannel> {
                    new UavcanChannel(RevolveUavcan.Dsdl.Types.BaseType.SIGNED_INT, 8, "success")
            };

            var values = new List<double> { 1 };
            var data = new byte[] { 1 };
            uint subjectId = 35;

            UavcanService service = new UavcanService(serializationRequestRules, serializationResponseRules, subjectId, "RTDS");

            yield return new object[] { new UavcanFrame() { Data = data, SubjectId = subjectId, IsServiceNotMessage = true, IsRequestNotResponse = true }, service, serializationRequestRules, subjectId, values };

            yield return new object[] { new UavcanFrame() { Data = data, SubjectId = subjectId, IsServiceNotMessage = true, IsRequestNotResponse = false }, service, serializationResponseRules, subjectId, values };
        }
    }
}
