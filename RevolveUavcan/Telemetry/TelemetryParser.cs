using RevolveUavcan.Telemetry.DataPackets;
using RevolveUavcan.Uavcan;
using System;

namespace RevolveUavcan.Telemetry
{

    /// <summary>
    /// Handles parsing of data packets (Multiple parsers, is abstract)
    /// Accepts
    /// </summary>
    public abstract class TelemetryParser
    {

        public static DateTime StartTime = DateTime.Now;

        /// <summary>
        /// The amount of packets that has been parsed
        /// </summary>
        public virtual int PacketsParsed { get; set; }

        /// <summary>
        /// Finished parsed packet of data
        /// </summary>
        public event EventHandler<DataPacket> DataPacketParsed;
        public event EventHandler<UavcanDataPacket> ServicePacketParsed;
        public event EventHandler<AlivePacket> AliveMessageParsed;
        public event EventHandler<UavcanDataPacket> UavcanPacketParsed;
        public event EventHandler<UavcanDataPacket> UavcanWarningReceived;
        public event EventHandler<UavcanFrame> UavcanServiceSent;
        public event EventHandler<UavcanFrame> UavcanMessageSent;



        public void PacketParsedEventInvoker(DataPacket dataPacket) => DataPacketParsed?.Invoke(this, dataPacket);

        public void ServiceReceivedInvoker(UavcanDataPacket packet) => ServicePacketParsed?.Invoke(this, packet);

        public void UavcanMessageEventInvoker(UavcanDataPacket packet) => UavcanPacketParsed?.Invoke(this, packet);

        public void AliveMessageParsedInvoker(AlivePacket packet) => AliveMessageParsed?.Invoke(this, packet);

        public void UavcanWarningReceivedInvoker(UavcanDataPacket packet) => UavcanWarningReceived?.Invoke(this, packet);

        public void UavcanServiceSentInvoker(UavcanFrame frame) => UavcanServiceSent?.Invoke(this, frame);

        public void UavcanMessageSentInvoker(UavcanFrame frame) => UavcanMessageSent?.Invoke(this, frame);


        /// <summary>
        /// Called to get parsed data from OnData with CANParser / F1Parser
        /// </summary>
        /// <param name="module">The ITelemetryModule which the data came from</param>
        public void RegisterOnDataEvent(ITelemetryModule module) => module.DataReceived += OnData;

        /// <summary>
        /// Stop recieving data over CANParser / F1Parser
        /// To be called upon opening a log, as analyze will crash
        /// </summary>
        /// <param name="module">The ITelemetryModule which the data came from</param>
        public void DeregisterOnDataEvent(ITelemetryModule module) => module.DataReceived -= OnData;

        /// <summary>
        /// Called when data has been received from any of the telemetry modules (e.g. serial, socket, file)
        /// </summary>
        /// <param name="sender">The ITelemetryModule which the data came from</param>
        /// <param name="args">An object containing the data received</param>
        public abstract void OnData(object sender, DataReceivedEventArgs args);

        /// <summary>
        /// Sends databuffer and count to parser functions and returns finished datapacket(s)
        /// </summary>
        /// <param databuffer="messages"></param>
        /// <param count="count"></param>
        /// public virtual void ByteStreamParse(IList<byte> messages, int count
        public virtual void ByteStreamParse(byte[] messages, int count)
        {

        }
    }
}
