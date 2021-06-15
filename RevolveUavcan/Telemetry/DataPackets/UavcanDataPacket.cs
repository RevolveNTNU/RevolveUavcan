using System.Collections.Generic;
using RevolveUavcan.Dsdl.Fields;
using RevolveUavcan.Uavcan;

namespace RevolveUavcan.Telemetry.DataPackets
{
    public class UavcanDataPacket
    {
        public Dictionary<UavcanChannel, double> ParsedDataDict { get; set; }

        public UavcanFrame UavcanFrame { get; set; }
    }
}
