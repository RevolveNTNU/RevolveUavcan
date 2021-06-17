using RevolveUavcan.Telemetry.DataPackets;
using System;
using System.Collections.Generic;
using System.Text;

namespace RevolveUavcan.Uavcan
{
    interface IUavcanParser
    {
        event EventHandler<UavcanDataPacket> UavcanMessageParsed;
        event EventHandler<UavcanDataPacket> UavcanServiceParsed;

        void ParseUavcanFrame(object sender, UavcanFrame frame);
    }
}
