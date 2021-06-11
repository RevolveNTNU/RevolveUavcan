using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RevolveUavcan.Telemetry
{
    /// <summary>
    ///     Represents one specific DataChannel. 1 CANid can have more than 1 DataChannel. The DataChannel.Name is a unique
    ///     identifier.
    /// </summary>
    [Serializable]
    public class DataChannel
    {
        public static string REVOLVE_DSDL = "Revolve DSDL";
        public static string UAVCAN_DSDL = "UAVCAN DSDL";
        public static string REVOLVE_PROTO = "Revolve Protocol";

        public static List<string> NAMESPACES = new List<string>
            { "ams", "ccc", "dashboard", "common", "sensors", "vcu", "tv" };

        public DataChannel(string name, string logName = "", long databaseSeriesId = -1)
        {
            Name = name;
            Category = GetCategory();
            if (name.Contains('.'))
            {
                Category = "DSDL";
                if (name.Split('_').First() == "request")
                {
                    NameSpace = "requests";
                    Message = name.Split('.')[1];
                }
                else if (name.Split('_').First() == "response")
                {
                    NameSpace = "responses";
                    Message = name.Split('.')[1];
                }
                else
                {
                    NameSpace = string.Join(".", name.Split('.').Take(1));
                    Message = name.Split('.')[1];
                }
            }
            else
            {
                NameSpace = "Revolve Protocol";
                Message = name.Split('_')[0];
                Category = GetCategory();
            }
            this.IsCustom = false;
            this.LogName = logName;
            this.SeriesId = databaseSeriesId;
        }


        /// <summary>
        ///     The name of this DataChannel. E.g. "BATTERY_TEMPERATURE_CELL_032" or "vcu.INS.vx"
        /// </summary>
        public string Name { get; }

        public string NameSpace { get; }

        /// <summary>
        ///     The CANID used by this DataChannel.
        /// </summary>
        public short CANID { get; set; }

        /// <summary>
        ///     The unit of the data in this channel. E.g. rmp, m/s, ...
        /// </summary>
        public string Unit { get; set; }
        public int StartByte { get; set; }
        public int Length { get; set; }
        public double Factor { get; set; }
        public string Type { get; set; }

        public bool IsCustom { get; set; }

        public string LogName { get; set; }
        public string Message { get; }
        public string Category { get; set; }
        public long SeriesId { get; set; }

        /// <summary>
        /// If the channel is a DSDL message, the shortname is the last part of the fullname
        /// Revolve CAN messages have no defined shortname
        /// </summary>
        public string ShortName
        {
            get
            {
                if (Category == "DSDL")
                {
                    return Name.Split('.')[^1];
                }
                else
                {
                    return Name;
                }

            }
        }


        /// <summary>
        ///     Converts the number at the end of a channelName into an int.
        /// </summary>
        /// <param name="d">The datachannel</param>
        /// <returns>
        ///     The int corresponding to the number after the last _ in the <see cref="Name" />. If there is not found a
        ///     number at the end of the name, -1 is returned
        /// </returns>
        public int ConvertChannelNumberToInt()
        {
            // TODO: Compile regex for speed
            var regexPattern = @"\w+_(\d+)";
            var regex = new Regex(regexPattern);
            var match = regex.Match(Name);
            if (match.Success)
            {
                if (int.TryParse(match.Groups[match.Groups.Count - 1].ToString(), out var number))
                {
                    return number;
                }
            }

            return -1;
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (GetType() != obj.GetType())
            {
                return false;
            }

            return Equals((DataChannel)obj);
        }

        protected bool Equals(DataChannel other) => string.Equals(Name, other.Name);

        public override int GetHashCode() => Name?.GetHashCode() ?? 0;

        public string GetCategory() => Name.Split('_')[0];
    }
}
