﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolveUavcan.Uavcan;
using System;
using System.Collections;
using System.Collections.Generic;
using static RevolveUavcan.Uavcan.UavcanFrame;

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

            Assert.AreEqual(0, uavcanFrame.TimeStamp);
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

        [DataTestMethod]
        [DynamicData(nameof(GetUavcanFrames), DynamicDataSourceType.Method)]
        public void UavcanFrameGetCanIdTest(UavcanFrame frame, uint expectedCanId)
        {
            Assert.AreEqual(expectedCanId, frame.GetCanId());
        }

        public static IEnumerable<object[]> GetUavcanFrames()
        {
            yield return new object[] { new UavcanFrame(){
                SubjectId = 413,
                Priority = 1,
                IsServiceNotMessage = false,
                SourceNodeId = 1
            }, (uint)73506049};

            yield return new object[] { new UavcanFrame(){
                SubjectId = 35,
                Priority = 1,
                IsServiceNotMessage = true,
                IsRequestNotResponse = true,
                SourceNodeId = 1,
                DestinationNodeId = 2,
            }, (uint)118014209};

            yield return new object[] { new UavcanFrame(){
                SubjectId = 35,
                Priority = 1,
                IsServiceNotMessage = true,
                IsRequestNotResponse = false,
                SourceNodeId = 2,
                DestinationNodeId = 1,
            }, (uint)101236866};
        }

        [DataTestMethod]
        [DynamicData(nameof(GetUavcanFramesWithTailByte), DynamicDataSourceType.Method)]
        public void UavcanFrameGetTailByteTest(UavcanFrame frame, byte expectedTailbyte)
        {
            Assert.AreEqual(expectedTailbyte, frame.GetTailByte());
        }

        public static IEnumerable<object[]> GetUavcanFramesWithTailByte()
        {
            yield return new object[] { new UavcanFrame(){
                IsStartOfTransfer = false,
                IsEndOfTransfer = false,
                ToggleBit = true,
                TransferId = 1
            }, (byte)33};

            yield return new object[] { new UavcanFrame(){
                IsStartOfTransfer = true,
                IsEndOfTransfer = true,
                ToggleBit = true,
                TransferId = 1
            }, (byte)225};
            yield return new object[] { new UavcanFrame(){
                IsStartOfTransfer = true,
                IsEndOfTransfer = false,
                ToggleBit = true,
                TransferId = 1
            }, (byte)161};
            yield return new object[] { new UavcanFrame(){
                IsStartOfTransfer = false,
                IsEndOfTransfer = true,
                ToggleBit = true,
                TransferId = 1
            }, (byte)97};
            yield return new object[] { new UavcanFrame(){
                IsStartOfTransfer = true,
                IsEndOfTransfer = true,
                ToggleBit = true,
                TransferId = 2
            }, (byte)226};
        }


        [TestMethod]
        [DynamicData(nameof(GetUavcanPayloadBytes), DynamicDataSourceType.Method)]
        public void UavcanFrameConstructorMultiframeTest(byte[] payload, FrameType expectedFrameType)
        {
            var frame = new UavcanFrame(new BitArray(32), payload, 0);
            Assert.AreEqual(expectedFrameType, frame.Type);
        }


        public static IEnumerable<object[]> GetUavcanPayloadBytes()
        {
            yield return new object[] { new byte[] { 32, 0b10100000 }, FrameType.MultiFrameStart };
            yield return new object[] { new byte[] { 32, 0b00000000 }, FrameType.MultiFrameMiddle };
            yield return new object[] { new byte[] { 32, 0b00100000 }, FrameType.MultiFrameMiddle };
            yield return new object[] { new byte[] { 32, 0b01000000 }, FrameType.MultiFrameEnd };
            yield return new object[] { new byte[] { 32, 0b01100000 }, FrameType.MultiFrameEnd };
        }


        [TestMethod]
        [DynamicData(nameof(GetInvalidUavcanPayloadBytes), DynamicDataSourceType.Method)]
        public void UavcanFrameIllegalTailByteThrows(byte[] payload)
        {
            Assert.ThrowsException<UavcanException>(() => new UavcanFrame(new BitArray(32), payload, 0));
        }

        public static IEnumerable<object[]> GetInvalidUavcanPayloadBytes()
        {
            yield return new object[] { new byte[] { 32, 0b10000000 } };
            yield return new object[] { new byte[] { 32, 0b11000000 } };
        }


        [TestMethod]
        [DynamicData(nameof(GetSingleFrames), DynamicDataSourceType.Method)]
        public void UavcanFrameInvalidAppendThrows(UavcanFrame frame1, UavcanFrame frame2)
        {
            Assert.ThrowsException<UavcanException>(() => frame1.AppendFrame(frame2));
        }
        public static IEnumerable<object[]> GetSingleFrames()
        {
            yield return new object[] { new UavcanFrame { Type = FrameType.SingleFrame }, new UavcanFrame { Type = FrameType.SingleFrame } };
            yield return new object[] { new UavcanFrame { Type = FrameType.SingleFrame }, new UavcanFrame { Type = FrameType.MultiFrameMiddle } };
            yield return new object[] { new UavcanFrame { Type = FrameType.SingleFrame }, new UavcanFrame { Type = FrameType.MultiFrameEnd } };
            yield return new object[] { new UavcanFrame { Type = FrameType.MultiFrameMiddle }, new UavcanFrame { Type = FrameType.SingleFrame } };
            yield return new object[] { new UavcanFrame { Type = FrameType.MultiFrameEnd }, new UavcanFrame { Type = FrameType.SingleFrame } };
            yield return new object[] { new UavcanFrame { Type = FrameType.MultiFrameStart, ToggleBit = true }, new UavcanFrame { Type = FrameType.MultiFrameMiddle, ToggleBit = true } };
        }

        [TestMethod]
        [DynamicData(nameof(GetInvalidHeader), DynamicDataSourceType.Method)]
        public void UavcanFrameInvalidHeaderThrows(BitArray headerBits)
        {
            Assert.ThrowsException<UavcanException>(() => new UavcanFrame(headerBits, new byte[12], 0));
        }
        public static IEnumerable<object[]> GetInvalidHeader()
        {
            yield return new object[] { new BitArray(20) };
        }

    }
}
