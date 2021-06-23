using System;
using System.Collections;
using System.Collections.Generic;
using NLog;
using RevolveUavcan.Dsdl.Fields;
using RevolveUavcan.Dsdl.Types;
using RevolveUavcan.Communication.DataPackets;
using RevolveUavcan.Tools;

namespace RevolveUavcan.Uavcan
{
    public class UavcanParser : IUavcanParser
    {
        private readonly ILogger _logger;
        private readonly UavcanSerializationRulesGenerator _dsdlRuleGenerator;
        private List<uint> _invalidMessageIds = new List<uint>();
        private List<uint> _invalidServiceIds = new List<uint>();

        public event EventHandler<UavcanDataPacket> UavcanMessageParsed;
        public event EventHandler<UavcanDataPacket> UavcanServiceParsed;

        /// <summary>
        /// Constructor for UAVCAN Parser. Registers the dsdl rules and subscribes to the framestorage
        /// </summary>
        /// <param name="logger">Logger used for log output</param>
        /// <param name="dsdlRuleGenerator">DSDL rules to be used in parsing and serialising</param>
        /// <param name="frameStorage">Framestorage that will provide frames to be parsed</param>
        public UavcanParser(ILogger logger, UavcanSerializationRulesGenerator dsdlRuleGenerator, UavcanFrameStorage frameStorage)
        {
            _logger = logger;
            _dsdlRuleGenerator = dsdlRuleGenerator;
            frameStorage.UavcanPacketReceived += ParseUavcanFrame;
        }

        public void ParseUavcanFrame(object sender, UavcanFrame frame)
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
            if (_dsdlRuleGenerator.TryGetSerializationRuleForMessage(frame.SubjectId, out var uavcanChannels))
            {
                var dataDictionary = ParseUavcanFrame(frame, uavcanChannels);

                // Initialize packet
                UavcanDataPacket uavcanPacket =
                    new UavcanDataPacket() { UavcanFrame = frame, ParsedDataDict = dataDictionary };

                UavcanMessageParsed?.Invoke(this, uavcanPacket);
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
            if (_dsdlRuleGenerator.TryGetSerializationRuleForService(frame.SubjectId, out var uavcanService))
            {
                var uavcanChannels = frame.IsRequestNotResponse
                    ? uavcanService.RequestFields
                    : uavcanService.ResponseFields;

                // Parse the frame and initialize data dictionary
                var dataDictionary = ParseUavcanFrame(frame, uavcanChannels);

                // Initialize ServicePacket
                UavcanDataPacket servicePacket = new UavcanDataPacket() { UavcanFrame = frame, ParsedDataDict = dataDictionary };
                UavcanServiceParsed?.Invoke(this, servicePacket);
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

        private Dictionary<UavcanChannel, double> ParseUavcanFrame(UavcanFrame frame, List<UavcanChannel> channels)
        {
            Dictionary<UavcanChannel, double> dataDictionary = new Dictionary<UavcanChannel, double>();

            // Bit offset keeps track of how many bits of the entire sequence has been parsed
            int bitOffset = 0;

            BitArray bitArray = new BitArray(frame.Data);

            foreach (UavcanChannel channel in channels)
            {
                // Void bits are just used for padding, and do not contain information
                if (channel.Basetype != BaseType.VOID)
                {
                    var result = ParseUavcanChannel(bitArray, channel, bitOffset);

                    var prefix = frame.IsServiceNotMessage ? (frame.IsRequestNotResponse ? UavcanSerializationRulesGenerator.REQUEST_PREFIX : UavcanSerializationRulesGenerator.RESPONSE_PREFIX) : "";

                    dataDictionary.Add(channel, result);
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
    }
}
