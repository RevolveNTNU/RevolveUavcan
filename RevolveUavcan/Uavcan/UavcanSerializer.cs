using RevolveUavcan.Dsdl.Fields;
using RevolveUavcan.Dsdl.Types;
using RevolveUavcan.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RevolveUavcan.Uavcan
{
    public class UavcanSerializer
    {
        /// <summary>
        /// Serializes as uavcan frame using provided uavcan channels, channelvalues and frame with ID
        /// </summary>
        /// <param name="uavcanChannels"></param>
        /// <param name="channelValues"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static UavcanFrame SerializeUavcanData(List<UavcanChannel> uavcanChannels, List<double> channelValues, UavcanFrame frame)
        {
            BitArray dataBits = BitArrayTools.GetBitArrayForUavcanChannels(uavcanChannels);

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
                            // To ensure no operation has been performed on the bool to change it from 1D/0D,
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

            return frame;
        }
    }
}
