using System;

namespace RevolveUavcan.Telemetry.DataPackets
{
    public class AlivePacket
    {

        public uint health;
        public uint uptime;
        public uint mode;
        public uint statusCode;
        public int sourceNodeId;

        public AlivePacket(uint health, uint uptime, uint mode, uint statusCode, int sourceNodeId)
        {
            this.health = health;
            this.uptime = uptime;
            this.mode = mode;
            this.statusCode = statusCode;
            this.sourceNodeId = sourceNodeId;
        }

        public AlivePacket(DataPacket packet, int sourceNodeId)
        {
            foreach (var key in packet.data.Keys)
            {
                if (key.Name.Contains("health"))
                {
                    health = Convert.ToUInt32(packet.data[key]);
                }
                else if (key.Name.Contains("uptime"))
                {
                    uptime = Convert.ToUInt32(packet.data[key]);
                }
                else if (key.Name.Contains("mode"))
                {
                    mode = Convert.ToUInt32(packet.data[key]);
                }
                else if (key.Name.Contains("status_code"))
                {
                    statusCode = Convert.ToUInt32(packet.data[key]);
                }
            }
            this.sourceNodeId = sourceNodeId;
        }
    }
}
