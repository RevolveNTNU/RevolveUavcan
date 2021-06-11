using System;

namespace RevolveUavcan.Telemetry
{
    public class DataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// The buffer containing the data
        /// </summary>
        public byte[] DataBuffer { get; }

        /// <summary>
        /// The amount of bytes to read from the buffer
        /// </summary>
        public int Count { get; }

        public bool FromPCan { get; }
        public int CanID { get; }
        /// <summary>
        /// This is the data recived event for the standard can protocol, and gets invoked in the modules (UDP, PCAN, KCAN etc)
        /// only gets invoked if it is a 11 bit standard frame (CAN 2.0A) or a pcan FD / KCAN FD
        /// </summary>
        /// <param name="dataBuffer">the data in the frame</param>
        /// <param name="count">lenght of data</param>
        /// <param name="fromPCan">if it is from pcan</param>
        /// <param name="canID">ID of the frame</param>
        public DataReceivedEventArgs(byte[] dataBuffer, int count, bool fromPCan = false, int canID = 0)
        {
            DataBuffer = dataBuffer;
            Count = count;
            FromPCan = fromPCan;
            CanID = canID;
        }
    }
}
