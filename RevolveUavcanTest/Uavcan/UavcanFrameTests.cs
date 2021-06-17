using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolveUavcan.Uavcan;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RevolveUavcanTest.Uavcan
{
    [TestClass]
    public class UavcanFrameTests
    {
        [DataTestMethod]
        [DynamicData(nameof(GetUavcanFrameInputs), DynamicDataSourceType.Method)]
        public void InitUavcanFrameTest(BitArray headerBits,
                                byte[] payload,
                                uint expectedSubjectId,
                                bool expectedIsServiceNotMessage,
                                bool expectedIsRequestNotResponse,
                                bool expectedIsStartOfTransfer,
                                bool expectedIsEndOfTransfer,
                                bool expectedIsComplete,
                                uint expectedSourceNodeId,
                                uint expectedDestinationNodeId)
        {
            var uavcanFrame = new UavcanFrame(headerBits, payload, 0);

            Assert.AreEqual(expectedSubjectId, uavcanFrame.SubjectId);
            Assert.AreEqual(expectedIsServiceNotMessage, uavcanFrame.IsServiceNotMessage);
            Assert.AreEqual(expectedIsRequestNotResponse, uavcanFrame.IsRequestNotResponse);
            Assert.AreEqual(expectedIsStartOfTransfer, uavcanFrame.IsStartOfTransfer);
            Assert.AreEqual(expectedIsEndOfTransfer, uavcanFrame.IsEndOfTransfer);
            Assert.AreEqual(expectedIsComplete, uavcanFrame.IsCompleted);
            Assert.AreEqual(expectedSourceNodeId, uavcanFrame.SourceNodeId);
            Assert.AreEqual(expectedDestinationNodeId, uavcanFrame.DestinationNodeId);
        }

        public static IEnumerable<object[]> GetUavcanFrameInputs()
        {
            // Pitot tube message, simple message
            yield return new object[] { new BitArray(BitConverter.GetBytes(73506175)),
                                        new byte[] { 0, 64, 154, 68, 0, 224 },
                                        (uint)413,
                                        false,
                                        true,
                                        true,
                                        true,
                                        true,
                                        (uint)127,
                                        (uint)0};

            // RTDS service, request part. From Dash to VCU
            yield return new object[] { new BitArray(BitConverter.GetBytes(118014475)),
                                        new byte[] { 1, 224 },
                                        (uint)35,
                                        true,
                                        true,
                                        true,
                                        true,
                                        true,
                                        (uint)11,
                                        (uint)4};

            // RTDS service, response part. From VCU to Dash
            yield return new object[] { new BitArray(BitConverter.GetBytes(101238148)),
                                        new byte[] { 1, 224 },
                                        (uint)35,
                                        true,
                                        false,
                                        true,
                                        true,
                                        true,
                                        (uint)4,
                                        (uint)11};
        }
    }
}
