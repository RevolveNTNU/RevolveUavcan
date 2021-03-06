using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RevolveUavcan.Communication;
using RevolveUavcan.Dsdl.Fields;
using RevolveUavcan.Uavcan;
using System.Collections.Generic;
using System.Linq;

namespace RevolveUavcanTest.Uavcan
{
    [TestClass]
    public class UavcanFrameStorageTests
    {
        private Mock<ILogger<UavcanFrameStorage>> frameStorageLoggerMock;
        private Mock<IUavcanCommunicationModule> uavcanCommMock;

        [TestInitialize]
        public void Setup()
        {
            frameStorageLoggerMock = new Mock<ILogger<UavcanFrameStorage>>();
            uavcanCommMock = new Mock<IUavcanCommunicationModule>();
        }


        [TestMethod]
        [DynamicData(nameof(GetUavcanSingleFrames), DynamicDataSourceType.Method)]
        public void ReceiveSingleFrameTest(UavcanFrame frame)
        {
            UavcanFrameStorage uavcanFrameStorage = new UavcanFrameStorage(frameStorageLoggerMock.Object);

            List<UavcanFrame> uavcanFrames = new List<UavcanFrame>();

            uavcanFrameStorage.RegisterOnDataEvent(uavcanCommMock.Object);

            uavcanFrameStorage.UavcanPacketReceived += delegate (object sender, UavcanFrame frame)
            {
                uavcanFrames.Add(frame);
            };

            uavcanCommMock.Raise(_ => _.UavcanFrameReceived += null, this, frame);

            Assert.AreEqual(1, uavcanFrames.Count);

            Assert.AreEqual(frame, uavcanFrames[0]);
        }

        public static IEnumerable<object[]> GetUavcanSingleFrames()
        {
            // Simple Pitot Tube message
            var data = new byte[] { 215, 163, 0, 67, 1 };
            uint subjectId = 413;

            yield return new object[] {
                new UavcanFrame() {
                    Data = data,
                    SubjectId = subjectId,
                    IsServiceNotMessage = false,
                    Type = UavcanFrame.FrameType.SingleFrame
                }
            };
        }

        [TestMethod]
        [DynamicData(nameof(GetUavcanMultiFrames), DynamicDataSourceType.Method)]
        public void ReceiveMultiFrameTest(List<UavcanFrame> multiFrames, UavcanFrame expectedFrame)
        {
            UavcanFrameStorage uavcanFrameStorage = new UavcanFrameStorage(frameStorageLoggerMock.Object);

            List<UavcanFrame> uavcanFrames = new List<UavcanFrame>();

            uavcanFrameStorage.UavcanPacketReceived += delegate (object sender, UavcanFrame frame)
            {
                uavcanFrames.Add(frame);
            };

            foreach (var frame in multiFrames)
            {
                Assert.IsTrue(uavcanFrames.Count == 0);
                uavcanFrameStorage.StoreFrame(uavcanCommMock, frame);
            }

            Assert.AreEqual(1, uavcanFrames.Count);
            CollectionAssert.AreEquivalent(expectedFrame.Data, uavcanFrames[0].Data);
        }

        public static IEnumerable<object[]> GetUavcanMultiFrames()
        {
            // Fake multiframe message
            var data = new byte[] { 215, 163, 0, 67 };
            uint subjectId = 413;
            byte transferId = 5;

            var frameStart = new UavcanFrame()
            {
                Data = data,
                SubjectId = subjectId,
                IsServiceNotMessage = false,
                Type = UavcanFrame.FrameType.MultiFrameStart,
                TransferId = transferId,
                ToggleBit = true
            };
            var frameMid = new UavcanFrame()
            {
                Data = data,
                SubjectId = subjectId,
                IsServiceNotMessage = false,
                Type = UavcanFrame.FrameType.MultiFrameMiddle,
                TransferId = transferId,
                ToggleBit = false
            };
            var frameEnd = new UavcanFrame()
            {
                Data = data,
                SubjectId = subjectId,
                IsServiceNotMessage = false,
                Type = UavcanFrame.FrameType.MultiFrameEnd,
                TransferId = transferId,
                ToggleBit = true
            };

            yield return new object[] {
                new List<UavcanFrame>() {
                    frameStart,
                    frameMid,
                    frameEnd
                },
                new UavcanFrame()
                {
                    Data = data.Concat(data).Concat(data).ToArray(),
                    SubjectId = subjectId,
                    IsServiceNotMessage = false,
                    Type = UavcanFrame.FrameType.MultiFrameEnd,
                    TransferId = transferId,
                    ToggleBit = true,
                    IsCompleted = true
                }
            };


            // Test that old start frame is discarded

            var frameStart2 = new UavcanFrame()
            {
                Data = data,
                SubjectId = subjectId,
                IsServiceNotMessage = false,
                Type = UavcanFrame.FrameType.MultiFrameStart,
                TransferId = transferId,
                ToggleBit = true
            };

            yield return new object[] {
                new List<UavcanFrame>() {
                    frameStart,
                    frameStart2,
                    frameMid,
                    frameEnd
                },
                new UavcanFrame()
                {
                    Data = data.Concat(data).Concat(data).ToArray(),
                    SubjectId = subjectId,
                    IsServiceNotMessage = false,
                    Type = UavcanFrame.FrameType.MultiFrameEnd,
                    TransferId = transferId,
                    ToggleBit = true,
                    IsCompleted = true
                }
            };
        }

        [TestMethod]
        [DynamicData(nameof(GetInvalidUavcanMultiFrames), DynamicDataSourceType.Method)]
        public void GetInvalidUavcanMultiFramesTest(List<UavcanFrame> multiFrames)
        {
            UavcanFrameStorage uavcanFrameStorage = new UavcanFrameStorage(frameStorageLoggerMock.Object);

            List<UavcanFrame> uavcanFrames = new List<UavcanFrame>();

            uavcanFrameStorage.UavcanPacketReceived += delegate (object sender, UavcanFrame frame)
            {
                Assert.Fail("An invalid frame should not be sent to the parser");
            };

            foreach (var frame in multiFrames)
            {
                uavcanFrameStorage.StoreFrame(uavcanCommMock, frame);
            }
        }

        public static IEnumerable<object[]> GetInvalidUavcanMultiFrames()
        {
            // Fake multiframe message
            var data = new byte[] { 215, 163, 0, 67 };
            uint subjectId = 413;
            byte transferId = 5;

            // Invalid togglebit on end frame
            yield return new object[] {
                new List<UavcanFrame>() {
                    new UavcanFrame(){Data = data, SubjectId = subjectId, IsServiceNotMessage = false,
                        Type = UavcanFrame.FrameType.MultiFrameStart,
                        TransferId = transferId,
                        ToggleBit = false
                    },
                    new UavcanFrame(){Data = data, SubjectId = subjectId, IsServiceNotMessage = false,
                        Type = UavcanFrame.FrameType.MultiFrameMiddle,
                        TransferId = transferId,
                        ToggleBit = false
                    },
                    new UavcanFrame(){Data = data, SubjectId = subjectId, IsServiceNotMessage = false,
                        Type = UavcanFrame.FrameType.MultiFrameEnd,
                        TransferId = transferId,
                        ToggleBit = false
                    }
                }
            };

            // Invalid togglebit on middle
            yield return new object[] {
                new List<UavcanFrame>() {
                    new UavcanFrame(){Data = data, SubjectId = subjectId, IsServiceNotMessage = false,
                        Type = UavcanFrame.FrameType.MultiFrameStart,
                        TransferId = transferId,
                        ToggleBit = true
                    },
                    new UavcanFrame(){Data = data, SubjectId = subjectId, IsServiceNotMessage = false,
                        Type = UavcanFrame.FrameType.MultiFrameMiddle,
                        TransferId = transferId,
                        ToggleBit = true
                    },
                    new UavcanFrame(){Data = data, SubjectId = subjectId, IsServiceNotMessage = false,
                        Type = UavcanFrame.FrameType.MultiFrameEnd,
                        TransferId = transferId,
                        ToggleBit = true
                    }
                }
            };

            // Invalid togglebit on end frame
            yield return new object[] {
                new List<UavcanFrame>() {
                    new UavcanFrame(){Data = data, SubjectId = subjectId, IsServiceNotMessage = false,
                        Type = UavcanFrame.FrameType.MultiFrameStart,
                        TransferId = transferId,
                        ToggleBit = true
                    },
                    new UavcanFrame(){Data = data, SubjectId = subjectId, IsServiceNotMessage = false,
                        Type = UavcanFrame.FrameType.MultiFrameMiddle,
                        TransferId = transferId,
                        ToggleBit = false
                    },
                    new UavcanFrame(){Data = data, SubjectId = subjectId, IsServiceNotMessage = false,
                        Type = UavcanFrame.FrameType.MultiFrameEnd,
                        TransferId = transferId,
                        ToggleBit = false
                    }
                }
            };

            // Lost middle frame
            yield return new object[] {
                new List<UavcanFrame>() {
                    new UavcanFrame(){Data = data, SubjectId = subjectId, IsServiceNotMessage = false,
                        Type = UavcanFrame.FrameType.MultiFrameStart,
                        TransferId = transferId,
                        ToggleBit = true
                    },
                    new UavcanFrame(){Data = data, SubjectId = subjectId, IsServiceNotMessage = false,
                        Type = UavcanFrame.FrameType.MultiFrameEnd,
                        TransferId = transferId,
                        ToggleBit = true
                    }
                }
            };

            // Lost end frame
            yield return new object[] {
                new List<UavcanFrame>() {
                    new UavcanFrame(){Data = data, SubjectId = subjectId, IsServiceNotMessage = false,
                        Type = UavcanFrame.FrameType.MultiFrameStart,
                        TransferId = transferId,
                        ToggleBit = true
                    },
                    new UavcanFrame(){Data = data, SubjectId = subjectId, IsServiceNotMessage = false,
                        Type = UavcanFrame.FrameType.MultiFrameMiddle,
                        TransferId = transferId,
                        ToggleBit = false
                    }
                }
            };

            // Lost start frame
            yield return new object[] {
                new List<UavcanFrame>() {
                    new UavcanFrame(){Data = data, SubjectId = subjectId, IsServiceNotMessage = false,
                        Type = UavcanFrame.FrameType.MultiFrameMiddle,
                        TransferId = transferId,
                        ToggleBit = false
                    },
                    new UavcanFrame(){Data = data, SubjectId = subjectId, IsServiceNotMessage = false,
                        Type = UavcanFrame.FrameType.MultiFrameEnd,
                        TransferId = transferId,
                        ToggleBit = true
                    }
                }
            };

            // Lost start and mid frame
            yield return new object[] {
                new List<UavcanFrame>() {
                    new UavcanFrame(){Data = data, SubjectId = subjectId, IsServiceNotMessage = false,
                        Type = UavcanFrame.FrameType.MultiFrameEnd,
                        TransferId = transferId,
                        ToggleBit = true
                    }
                }
            };
        }

        [TestMethod]
        public void ResetFrameStorageBufferTest()
        {
            var data = new byte[] { 215, 163, 0, 67 };
            uint subjectId = 420;
            byte transferId = 5;

            UavcanFrameStorage uavcanFrameStorage = new UavcanFrameStorage(frameStorageLoggerMock.Object);

            List<UavcanFrame> uavcanFrames = new List<UavcanFrame>();

            uavcanFrameStorage.UavcanPacketReceived += delegate (object sender, UavcanFrame frame)
                {
                    uavcanFrames.Add(frame);
                };


            var frame1 = new UavcanFrame
            {
                Data = data,
                TransferId = transferId,
                SubjectId = subjectId,
                IsStartOfTransfer = true,
                IsEndOfTransfer = false,
                ToggleBit = true,
                Type = UavcanFrame.FrameType.MultiFrameStart
            };

            var frame1Copy = new UavcanFrame
            {
                Data = data,
                TransferId = transferId,
                SubjectId = subjectId,
                IsStartOfTransfer = true,
                IsEndOfTransfer = false,
                ToggleBit = true,
                Type = UavcanFrame.FrameType.MultiFrameStart
            };

            var frame2 = new UavcanFrame
            {
                Data = data,
                TransferId = transferId,
                SubjectId = subjectId,
                IsStartOfTransfer = false,
                IsEndOfTransfer = false,
                ToggleBit = false,
                Type = UavcanFrame.FrameType.MultiFrameMiddle
            };

            var frame2Copy = new UavcanFrame
            {
                Data = data,
                TransferId = transferId,
                SubjectId = subjectId,
                IsStartOfTransfer = false,
                IsEndOfTransfer = false,
                ToggleBit = false,
                Type = UavcanFrame.FrameType.MultiFrameMiddle
            };

            var frame3 = new UavcanFrame
            {
                Data = data,
                TransferId = transferId,
                SubjectId = subjectId,
                IsStartOfTransfer = false,
                IsEndOfTransfer = true,
                ToggleBit = true,
                Type = UavcanFrame.FrameType.MultiFrameEnd
            };

            var frame4 = new UavcanFrame
            {
                Data = data,
                TransferId = 7,
                SubjectId = subjectId,
                IsStartOfTransfer = false,
                IsEndOfTransfer = true,
                ToggleBit = true,
                Type = UavcanFrame.FrameType.MultiFrameEnd
            };


            uavcanFrameStorage.StoreFrame(uavcanCommMock, frame1);
            uavcanFrameStorage.StoreFrame(uavcanCommMock, frame2);
            uavcanFrameStorage.StoreFrame(uavcanCommMock, frame1Copy); // Try to store twice, should reset and only store once
            uavcanFrameStorage.StoreFrame(uavcanCommMock, frame2Copy);
            uavcanFrameStorage.StoreFrame(uavcanCommMock, frame3);

            Assert.AreEqual(1, uavcanFrames.Count);

            uavcanFrameStorage.StoreFrame(uavcanCommMock, frame4); // Try to add an Endframe, should not be added 

            Assert.AreEqual(1, uavcanFrames.Count);
        }
    }
}
