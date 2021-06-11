using RevolveUavcan.Uavcan;
using System.Collections.Generic;

namespace RevolveUavcan.Telemetry.DataPackets
{
    public class DataPacket
    {
        /// <summary>
        ///     Provides data packets with the value with which they must be timestampOffset
        ///     to avoid overwriting old data.
        /// </summary>
        private static long timestampOffset;

        /// <summary>
        ///     Set to true by <see cref="UpdateTimestampOffset" />, allows us to call <see cref="SetNewTimestampOffset" />from the
        ///     next created dataPacket.
        ///     Set back to false at the end of <see cref="SetNewTimestampOffset" />
        /// </summary>
        private static bool isUpdatingTimestampOffset;

        public readonly Dictionary<DataChannel, double> data;
        public readonly long timestamp;
        public byte canbus;
        public short id;
        public byte[] rawData;
        public int? signal;

        public DataPacket(long timestamp, Dictionary<DataChannel, double> data)
        {
            this.timestamp = timestamp;
            this.data = data;
            if (isUpdatingTimestampOffset)
            {
                SetNewTimestampOffset(timestamp);
            }
        }

        public DataPacket(UavcanFrame frame, Dictionary<DataChannel, double> dataDictionary)
        {
            this.timestamp = frame.TimeStamp;
            this.data = dataDictionary;
            if (isUpdatingTimestampOffset)
            {
                SetNewTimestampOffset(timestamp);
            }
        }

        #region Offset Timestamp

        /// <summary>
        ///     A modified timestamp value.  Offset so that it it's value isn't displayed among older data, even if the sender's
        ///     timestamps have been reset.
        /// </summary>
        public long OffsetTimestamp => timestamp + timestampOffset;


        /// <summary>
        ///     Call this method to update <see cref="timestampOffset" />, so that future messages will be displayed after the
        ///     current newest timestamp
        /// </summary>
        public static void UpdateTimestampOffset() => isUpdatingTimestampOffset = true;

        /// <summary>
        ///     Method sets <see cref="timestampOffset" /> so that the caller's <see cref="OffsetTimestamp" /> will be the same as
        ///     the current <see cref="SciChartDataModel.NewestTimestamp" />
        /// </summary>
        /// <param name="timestamp">The actual timestamp of the <see cref="DataPacket" /> which called the function</param>
        private static void SetNewTimestampOffset(long timestamp)
        {
            //TODO: Find out what this is....



            /*timestampOffset = EventWorker.Instance.AnalyzeDataModel.logDataModel.sciChartDataModel.NewestTimestamp -
                              timestamp;
            isUpdatingTimestampOffset = false;*/
        }

        /// <summary>
        ///     Sets <see cref="timestampOffset" /> back to 0
        /// </summary>
        public static void CancelTimestampOffset() => timestampOffset = 0;

        #endregion

        public override string ToString() => $"DataPacket {timestamp}";
    }
}
