using RevolveUavcan.Uavcan;
using System;

namespace RevolveUavcan.Telemetry
{
    public interface ITelemetryModule
    {
        bool Write(short canId, byte[] message);

        /// <summary>
        /// Is used for writing Uavcan messages to the bus.
        /// Is extended 29 bit identifier
        /// </summary>
        /// <param name="canId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        bool WriteEx(uint canId, byte[] data);

        bool WriteUavcanFrame(UavcanFrame frame);

        void Stop();

        bool IsConnected();

        event EventHandler<DataReceivedEventArgs> DataReceived;

        event EventHandler<UavcanFrame> UavcanDataReceived;
    }
}
