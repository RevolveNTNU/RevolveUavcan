using RevolveUavcan.Dsdl.Fields;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevolveUavcan.Dsdl.Types
{
    public enum MessageType
    {
        SERVICE,
        MESSAGE
    }

    public class CompoundType : DsdlType
    {


        public Tuple<int, int> Version { get; set; }

        public List<Field> RequestFields { get; set; }
        public List<Field> ResponseFields { get; set; }
        public List<Constant> RequestConstants { get; set; }
        public List<Constant> ResponseConstants { get; set; }
        public MessageType MessageType { get; set; }
        public uint SubjectId { get; set; }

        public bool ResponseUnion { get; set; } = false;
        public bool RequestUnion { get; set; } = false;

        public CompoundType(string fullName, MessageType messageType, uint subjectId,
                                Tuple<int, int> version) : base(fullName, Category.COMPOUND)
        {
            MessageType = messageType;
            SubjectId = subjectId;
            Version = version;

            RequestFields = new List<Field>();
            ResponseFields = new List<Field>();
            RequestConstants = new List<Constant>();
            ResponseConstants = new List<Constant>();
        }

        private int computeMaxLen(List<Field> fields, bool union)
        {
            if (fields.Count == 0)
            {
                return 0;
            }
            List<int> lengths = new List<int>();
            foreach (Field f in fields)
            {
                lengths.Add(f.type.GetMaxBitLength());
            }

            if (union)
            {
                return lengths.Max() + Convert.ToString(Math.Max(fields.Count - 1, 1), 2).Length;
            }

            return lengths.Sum();
        }

        private int computeMinLen(List<Field> fields, bool union)
        {
            if (fields.Count == 0)
            {
                return 0;
            }
            List<int> lengths = new List<int>();
            foreach (Field f in fields)
            {
                lengths.Add(f.type.GetMinBitLength());
            }

            if (union)
            {
                return lengths.Min() + Convert.ToString(Math.Max(fields.Count - 1, 1), 2).Length;
            }

            return lengths.Sum();
        }


        public override string ToString() => throw new System.NotImplementedException();

        public int GetMaxBitLengthRequest() => computeMaxLen(RequestFields, RequestUnion);

        public int GetMaxBitLengthResponse() => computeMaxLen(ResponseFields, ResponseUnion);

        public int GetMinBitLengthRequest() => computeMinLen(RequestFields, RequestUnion);

        public int GetMinBitLengthResponse() => computeMinLen(ResponseFields, ResponseUnion);

        public override int GetMinBitLength() => GetMinBitLengthRequest();

        public override int GetMaxBitLength() => GetMaxBitLengthRequest();
    }
}
