using RevolveUavcan.Uavcan;
using System;

namespace RevolveUavcan.Telemetry
{
    public interface ITelemetryModule
    {
        void Read();

        bool WriteUavcanFrame(UavcanFrame frame);

        void Stop();

        bool IsConnected();

        event EventHandler<UavcanFrame> UavcanFrameReceived;
    }
}
