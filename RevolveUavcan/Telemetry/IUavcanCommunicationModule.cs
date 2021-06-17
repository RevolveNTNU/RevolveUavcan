using RevolveUavcan.Uavcan;
using System;

namespace RevolveUavcan.Telemetry
{
    public interface IUavcanCommunicationModule
    {
        /// <summary>
        /// Event that is triggered whenever a UAVCAN frame has been read
        /// </summary>
        event EventHandler<UavcanFrame> UavcanFrameReceived;

        /// <summary>
        /// Method for writing a UAVCAN frame to a bus/connection
        /// </summary>
        /// <param name="frame">UAVCAN frame to be sent</param>
        /// <returns>True if successfully sent, false otherwise</returns>
        bool WriteUavcanFrame(UavcanFrame frame);
    }
}
