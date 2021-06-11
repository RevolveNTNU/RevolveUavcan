using System;
using System.Collections;
using System.Collections.Generic;
using NLog;
using RevolveUavcan.Dsdl;
using RevolveUavcan.Dsdl.Fields;
using RevolveUavcan.Dsdl.Types;
using RevolveUavcan.Telemetry;
using RevolveUavcan.Telemetry.DataPackets;
using RevolveUavcan.Tools;

namespace RevolveUavcan.Uavcan
{
    public class UavcanParser : TelemetryParser
    {
        private const uint ALIVE_ID = 7509;
        private List<uint> _invalidMessageIds = new List<uint>();
        private List<uint> _invalidServiceIds = new List<uint>();

        private readonly ILogger _logger;
        private readonly DsdlRuleGenerator _dsdlRuleGenerator;

        public UavcanParser(ILogger logger, DsdlRuleGenerator dsdlRuleGenerator)
        {
            _logger = logger;
            _dsdlRuleGenerator = dsdlRuleGenerator;
        }

        public void RegisterOnDataEvent(FrameStorage obj) => obj.UavcanPacketReceived += ParseDataFrame;

        private void ParseDataFrame(object sender, UavcanFrame frame)
        {
            if (frame.IsServiceNotMessage)
            {
                ParseService(frame);
            }
            else
            {
                ParseMessage(frame);
            }
        }

        private void ParseMessage(UavcanFrame frame)
        {
            if (_dsdlRuleGenerator.MessageDataIdMap.TryGetValue(frame.SubjectId,
                out var messageName))
            {
                var uavcanChannels =
                    _dsdlRuleGenerator.FlattenedDsdlMessages[messageName];

                var dataDictionary = ParseUavcanFrame(frame, uavcanChannels);

                // Initialize DataPacket
                DataPacket dataPacket = new DataPacket(frame.TimeStamp, dataDictionary);
                UavcanDataPacket uavcanPacket =
                    new UavcanDataPacket() { UavcanFrame = frame, ParsedDataDict = dataDictionary };

                PacketParsedEventInvoker(dataPacket);
                UavcanMessageEventInvoker(uavcanPacket);
                if (frame.SubjectId == ALIVE_ID)
                {
                    AliveMessageParsedInvoker(new AlivePacket(dataPacket, frame.SourceNodeId));
                }
                if (messageName.Contains("Warning"))
                {
                    UavcanWarningReceivedInvoker(uavcanPacket);
                }
            }
            else
            {
                // Only log this once per session
                if (!_invalidMessageIds.Contains(frame.SubjectId))
                {
                    // If we cannot find a DSDL mapping for this ID, we cannot parse the data. Frame is then discarded
                    _logger.Warn($"Unable to find a DSDL mapping for Message ID {frame.SubjectId}");

                    //Toast.Warning($"No DSDL mapping found for Message ID: {frame.SubjectId}");
                    _invalidMessageIds.Add(frame.SubjectId);
                }
            }
        }

        private void ParseService(UavcanFrame frame)
        {
            if (_dsdlRuleGenerator.ServiceDataIdMap.TryGetValue(frame.SubjectId,
                    out var serviceName) &&
                _dsdlRuleGenerator.FlattenedServices.TryGetValue(
                    serviceName, out var uavcanService))
            {
                var uavcanChannels = frame.IsRequestNotResponse
                    ? uavcanService.RequestFields
                    : uavcanService.ResponseFields;

                // Parse the frame and initialize data dictionary
                var dataDictionary = ParseUavcanFrame(frame, uavcanChannels);

                // Initialize ServicePacket
                UavcanDataPacket servicePacket =
                    new UavcanDataPacket() { UavcanFrame = frame, ParsedDataDict = dataDictionary };
                ServiceReceivedInvoker(servicePacket);

                DataPacket dataPacket = new DataPacket(frame.TimeStamp, dataDictionary);
                PacketParsedEventInvoker(dataPacket);
            }
            else
            {
                // Only log this once per session
                if (!_invalidServiceIds.Contains(frame.SubjectId))
                {
                    // If we cannot find a DSDL mapping for this ID, we cannot parse the data. Frame is then discarded
                    _logger.Warn($"Unable to find a DSDL mapping for Service ID {frame.SubjectId}");
                    _invalidServiceIds.Add(frame.SubjectId);
                }
            }
        }

        private Dictionary<DataChannel, double> ParseUavcanFrame(UavcanFrame frame, List<UavcanChannel> channels)
        {
            Dictionary<DataChannel, double> dataDictionary = new Dictionary<DataChannel, double>();

            // Bit offset keeps track of how many bits of the entire sequence has been parsed
            int bitOffset = 0;

            BitArray bitArray = new BitArray(frame.Data);

            foreach (UavcanChannel channel in channels)
            {
                // Void bits are just used for padding, and do not contain information
                if (channel.Basetype != BaseType.VOID)
                {
                    var result = ParseUavcanChannel(bitArray, channel, bitOffset);

                    var prefix = frame.IsServiceNotMessage ? (frame.IsRequestNotResponse ? DsdlRuleGenerator.REQUEST_PREFIX : DsdlRuleGenerator.RESPONSE_PREFIX) : "";

                    if (EventWorker.Instance.AnalyzeDataModel.DataChannels.TryGetValue(prefix + channel.FieldName,
                        out var dataChannel))
                    {
                        dataDictionary.Add(dataChannel, result);
                    }
                    else
                    {
                        // If we dont have a datachannel for the data, we cannot add it to the backend.
                        _logger
                            .Warn($"Unable to recognise UAVCAN message with field name {channel.FieldName}");
                    }
                }

                // Increment bit offset with the size of the parsed channel
                bitOffset += channel.Size;
            }

            return dataDictionary;
        }

        private double ParseUavcanChannel(BitArray frameData, UavcanChannel channel, int bitOffset)
        {
            double result = 0;

            // Split the bit sequence sequence from the current bit offset and channelSize amount of elements further
            BitArray dataBits = frameData.GetRange(bitOffset, channel.Size);

            result = channel.Basetype switch
            {
                BaseType.SIGNED_INT => dataBits.GetLongFromBitArray(),
                BaseType.UNSIGNED_INT => dataBits.GetUIntFromBitArray(),
                BaseType.BOOLEAN => dataBits.Get(0) ? 1D : 0D,
                BaseType.FLOAT => dataBits.GetFloatFromBitArray(),
                _ => result
            };

            return result;
        }

        public UavcanFrame SerializeUavcanFrame(
            List<UavcanChannel> uavcanChannels,
            List<double> channelValues,
            UavcanFrame frame)
        {
            BitArray dataBits =
                BitArrayTools.GetBitArrayForUavcanChannels(uavcanChannels);

            var listIndex = 0;
            var dataIndex = 0;

            foreach (UavcanChannel channel in uavcanChannels)
            {
                switch (channel.Basetype)
                {
                    case BaseType.VOID:
                        break;
                    case BaseType.FLOAT:
                        {
                            var bits = BitArrayTools.GetBitArrayFromDouble(channelValues[listIndex++], channel.Size);

                            // Insert float value as bits into dataBits BitArray
                            dataBits = dataBits.InsertRange(bits, dataIndex);
                            break;
                        }
                    case BaseType.BOOLEAN:
                        {
                            // To ensure no operation have been performed on the bool to change it from 1D/0D,
                            // we check that it is true/false by comparing with 0.5
                            dataBits[dataIndex] = channelValues[listIndex++] > 0.5;
                            break;
                        }
                    case BaseType.SIGNED_INT:
                        {
                            var bits = BitArrayTools.GetBitArrayFromLong((long)channelValues[listIndex++], channel.Size);

                            dataBits = dataBits.InsertRange(bits, dataIndex);
                            break;
                        }
                    case BaseType.UNSIGNED_INT:
                        {
                            var bits = BitArrayTools.GetBitArrayFromUInt((uint)channelValues[listIndex++], channel.Size);

                            dataBits = dataBits.InsertRange(bits, dataIndex);
                            break;
                        }
                }

                dataIndex += channel.Size;
            }

            frame.Data = dataBits.GetByteArrayFromBitArray();

            if (frame.IsServiceNotMessage)
            {
                UavcanServiceSentInvoker(frame);
            }
            else
            {
                UavcanMessageSentInvoker(frame);
            }
            return frame;
        }


        /// <summary>
        /// Implemented from parent class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public override void OnData(object sender, DataReceivedEventArgs args) => throw new NotImplementedException();
    }
}
