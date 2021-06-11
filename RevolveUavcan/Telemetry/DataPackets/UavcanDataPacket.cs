using System.Collections.Generic;
using RevolveUavcan.Uavcan;

namespace RevolveUavcan.Telemetry.DataPackets
{
    public class UavcanDataPacket
    {
        public Dictionary<DataChannel, double> ParsedDataDict { get; set; }

        public UavcanFrame UavcanFrame { get; set; }
    }
}
