using RevolveUavcan.Tools;
using System.Collections;
using System.Linq;

namespace RevolveUavcan.Uavcan
{
    public class UavcanFrame
    {
        /// <summary>
        /// Enum for the various types of frames
        /// </summary>
        public enum FrameType
        {
            MultiFrameStart,
            MultiFrameMiddle,
            MultiFrameEnd,
            SingleFrame,
        }

        public byte[] Data { get; set; }

        public int DataLength => Data.Length;
        public uint SubjectId { get; set; }
        public bool IsServiceNotMessage { get; set; }

        public bool IsRequestNotResponse { get; set; }

        public bool IsCompleted { get; set; }

        public uint Priority { get; set; }

        public bool IsStartOfTransfer { get; set; }
        public bool IsEndOfTransfer { get; set; }
        public bool ToggleBit { get; set; }
        public byte TransferId { get; set; }

        public long TimeStamp { get; set; }

        public FrameType Type { get; set; }

        public uint SourceNodeId { get; set; }

        public uint DestinationNodeId { get; set; }

        #region Constants

        private const int MESSAGE_ID_INDEX = 8;
        private const int MESSAGE_ID_LENGTH = 13;
        private const int SERVICE_ID_INDEX = 14;
        private const int SERVICE_ID_LENGTH = 9;
        private const int HEADER_BIT_LENGTH = 29;
        private const int SOURCE_NODE_INDEX = 0;
        private const int MESSAGE_SOURCE_NODE_ID_LENGTH = 7;
        private const int SERVICE_SOURCE_NODE_ID_LENGTH = 6;
        private const int DESTINATION_NODE_INDEX = 7;
        private const int IS_SERVICE_NOT_MESSAGE_INDEX = 25;
        private const int CAN_BIT_LENGTH = 32;
        private const int DESTINATION_NODE_LENGTH = 6;
        private const int IS_REQUEST_NOT_RESPONSE_INDEX = 24;
        private const int PRIORITY_INDEX = 26;
        private const int PRIORITY_LENGTH = 3;
        private const int SERVICE_RESERVED_BIT_INDEX = 23;
        private const int MESSAGE_RESERVED_BIT_INDEX_21 = 21;
        private const int MESSAGE_RESERVED_BIT_INDEX_22 = 22;
        private const int MESSAGE_RESERVED_BIT_INDEX_23 = 23;
        private const int MESSAGE_RESERVED_BIT_INDEX_24 = 24;

        #endregion

        public UavcanFrame()
        {
        }

        public UavcanFrame(BitArray headerBits, byte[] payload, long timeStamp)
        {
            // Initialize properties
            IsServiceNotMessage = headerBits.Get(IS_SERVICE_NOT_MESSAGE_INDEX);
            TimeStamp = timeStamp;
            IsRequestNotResponse = IsServiceNotMessage ? headerBits.Get(IS_REQUEST_NOT_RESPONSE_INDEX) : true;

            // Get subject ID bits from initial header bits
            BitArray subjectIdBits = IsServiceNotMessage
                ? headerBits.GetRange(SERVICE_ID_INDEX, SERVICE_ID_LENGTH)
                : headerBits.GetRange(MESSAGE_ID_INDEX, MESSAGE_ID_LENGTH);
            // Calculate subject ID based on subject ID bits
            SubjectId = subjectIdBits.GetUIntFromBitArray();


            // Calculate source node ID
            SourceNodeId =
                headerBits.GetRange(SOURCE_NODE_INDEX, MESSAGE_SOURCE_NODE_ID_LENGTH).GetUIntFromBitArray();

            // Calculate destination node ID
            DestinationNodeId = !IsServiceNotMessage
                ? 0
                : headerBits.GetRange(DESTINATION_NODE_INDEX, MESSAGE_SOURCE_NODE_ID_LENGTH).GetUIntFromBitArray();


            // Decode tail byte structure
            DecodeTailByte(payload.Last());

            // Get FrameType and evaluate whether it is completed
            Type = GetFrameType();
            IsCompleted = Type == FrameType.SingleFrame;

            // Remove tail byte from initial data
            Data = TrimTailByteFromData(payload);
        }

        /// <summary>
        /// See documentation, table 4.4: Tail byte structure. Link: https://uavcan.org/specification/UAVCAN_Specification_v1.0-beta.pdf.
        /// </summary>
        /// <param name="b"></param>
        private void DecodeTailByte(byte b)
        {
            IsStartOfTransfer = (b & (1 << 7)) != 0;
            IsEndOfTransfer = (b & (1 << 6)) != 0;
            ToggleBit = (b & (1 << 5)) != 0;
            TransferId = (byte)(b & 0x1F);
        }

        /// <summary>
        /// See documentation, table 4.4: Tail byte structure. Link: https://uavcan.org/specification/UAVCAN_Specification_v1.0-beta.pdf.
        /// </summary>
        /// <returns></returns>
        public byte GetTailByte()
        {
            var bytes = new byte[1];
            bytes[0] = TransferId;
            var bitArray = new BitArray(bytes);

            // Encode tail byte
            bitArray.Set(5, ToggleBit);
            bitArray.Set(6, IsEndOfTransfer);
            bitArray.Set(7, IsStartOfTransfer);

            return bitArray.GetByteArrayFromBitArray()[0];
        }

        /// <summary>
        /// See documentation, table 4.3: CAN ID bit layout. Link: https://uavcan.org/specification/UAVCAN_Specification_v1.0-beta.pdf.
        /// </summary>
        /// <returns>The unsigned integer representation of the BitArray</returns>
        public uint GetCanId()
        {
            // Encode CAN header bits following UAVCAN 1.0 specification documentation
            if (IsServiceNotMessage)
            {
                // Initialize header bits
                var bitArray = new BitArray(CAN_BIT_LENGTH);

                // Set Source node ID
                bitArray = bitArray.InsertRange(
                    BitArrayTools.GetBitArrayFromLong(SourceNodeId, SERVICE_SOURCE_NODE_ID_LENGTH), SOURCE_NODE_INDEX
                );

                // Set Destination node ID
                bitArray = bitArray.InsertRange(
                    BitArrayTools.GetBitArrayFromLong(DestinationNodeId, DESTINATION_NODE_LENGTH),
                    DESTINATION_NODE_INDEX
                );

                // Set Service ID
                bitArray = bitArray.InsertRange(
                    BitArrayTools.GetBitArrayFromUInt(SubjectId, SERVICE_ID_LENGTH),
                    SERVICE_ID_INDEX
                );

                // Set reserved bit
                bitArray.Set(SERVICE_RESERVED_BIT_INDEX, false);

                // Set is request, not response
                bitArray.Set(IS_REQUEST_NOT_RESPONSE_INDEX, IsRequestNotResponse);

                // Set is service, not message
                bitArray.Set(IS_SERVICE_NOT_MESSAGE_INDEX, IsServiceNotMessage);

                bitArray = bitArray.InsertRange(BitArrayTools.GetBitArrayFromLong(Priority, PRIORITY_LENGTH),
                    PRIORITY_INDEX
                );

                return bitArray.GetUIntFromBitArray();
            }
            else
            {
                // Initialize header bits
                var bitArray = new BitArray(CAN_BIT_LENGTH);

                // Set source node ID
                bitArray = bitArray.InsertRange(
                    BitArrayTools.GetBitArrayFromLong(SourceNodeId, MESSAGE_SOURCE_NODE_ID_LENGTH), SOURCE_NODE_INDEX
                );

                // Set subject ID
                bitArray = bitArray.InsertRange(BitArrayTools.GetBitArrayFromLong(SubjectId, MESSAGE_ID_LENGTH),
                    MESSAGE_ID_INDEX
                );

                // Set reserved bits
                bitArray.Set(MESSAGE_RESERVED_BIT_INDEX_21, true);
                bitArray.Set(MESSAGE_RESERVED_BIT_INDEX_22, true);
                bitArray.Set(MESSAGE_RESERVED_BIT_INDEX_23, false);
                bitArray.Set(MESSAGE_RESERVED_BIT_INDEX_24, false);

                // Set is service, not message
                bitArray.Set(IS_SERVICE_NOT_MESSAGE_INDEX, IsServiceNotMessage);

                bitArray = bitArray.InsertRange(BitArrayTools.GetBitArrayFromLong(Priority, PRIORITY_LENGTH),
                    PRIORITY_INDEX
                );

                return bitArray.GetUIntFromBitArray();
            }
        }


        /// <summary>
        /// Removes tail byte from data
        /// </summary>
        /// <param name="data">A byte array containing a tail byte we want to remove.</param>
        /// <returns>Byte array without tail byte</returns>
        private byte[] TrimTailByteFromData(byte[] data)
        {
            byte[] trimmed = new byte[data.Length - 1];

            for (int i = 0; i < trimmed.Length; i++)
            {
                trimmed[i] = data[i];
            }

            return trimmed;
        }

        /// <summary>
        /// Append a provided byte array to the byte array in UavcanFrame.
        /// Used for UavcanFrame with type MultiFrame
        /// </summary>
        /// <param name="nextFrame">The byte array to append to the byte array in UavcanFrame</param>
        public void AppendFrame(UavcanFrame nextFrame)
        {
            if (Type == FrameType.SingleFrame || nextFrame.Type == FrameType.SingleFrame)
            {
                throw new UavcanException("Cannot append SingleFrames");
            }
            IsStartOfTransfer = false;
            byte[] result = new byte[DataLength + nextFrame.DataLength];
            System.Buffer.BlockCopy(Data, 0, result, 0, DataLength);
            System.Buffer.BlockCopy(nextFrame.Data, 0, result, DataLength, nextFrame.DataLength);

            ToggleBit = nextFrame.ToggleBit;
            Data = result;
            IsCompleted = nextFrame.Type == FrameType.MultiFrameEnd;
            Type = nextFrame.Type;
        }

        private FrameType GetFrameType()
        {
            // All conditions are true when frame type is SingleFrame
            if (IsStartOfTransfer && IsEndOfTransfer && ToggleBit)
            {
                return FrameType.SingleFrame;
            }

            // This is the start of a MultiFrame. ToggleBit is always 1 in the start of MultiFrame.
            if (IsStartOfTransfer && !IsEndOfTransfer && ToggleBit)
            {
                return FrameType.MultiFrameStart;
            }

            // This is the end of a MultiFrame. ToggleBit alternates between 1 and 0 to verify next message.
            if (!IsStartOfTransfer && IsEndOfTransfer)
            {
                return FrameType.MultiFrameEnd;
            }

            // In UAVCAN v1, Toggle bit is 1 for the first frame. In Uavcan v0, the Toggle bit is 0
            if (IsStartOfTransfer && !ToggleBit)
            {
                throw new UavcanException("This Uavcan implementation does not support UAVCAN v0");
            }

            // In this case, MultiFrame is not yet finished and will receive more
            return FrameType.MultiFrameMiddle;
        }
    }
}
