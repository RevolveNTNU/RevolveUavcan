using RevolveUavcan.Communication.DataPackets;
using System;

namespace RevolveUavcan.Uavcan
{
    interface IUavcanParser
    {
        event EventHandler<UavcanDataPacket> UavcanMessageParsed;
        event EventHandler<UavcanDataPacket> UavcanServiceParsed;

        void ParseUavcanFrame(object sender, UavcanFrame frame);
    }
}
