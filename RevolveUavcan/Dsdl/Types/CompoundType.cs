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
        public MessageType messageType;
        private readonly string sourceFile;
        public uint defaultDataTypeID;
        private readonly Tuple<int, int> version;
        private readonly string sourceText;

        public List<Field> requestFields;
        public List<Field> responseFields;
        public List<Constant> requestConstants;
        public List<Constant> responseConstants;

        public bool responseUnion = false;
        public bool requestUnion = false;

        public CompoundType(string fullName, MessageType messageType, string sourceFile, uint defaultDataTypeID,
                        Tuple<int, int> version, string sourceText) : base(fullName, Category.COMPOUND)
        {
            this.messageType = messageType;
            this.sourceFile = sourceFile;
            this.defaultDataTypeID = defaultDataTypeID;
            this.version = version;
            this.sourceText = sourceText;

            requestFields = new List<Field>();
            responseFields = new List<Field>();
            requestConstants = new List<Constant>();
            responseConstants = new List<Constant>();
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

        public int GetMaxBitLengthRequest() => computeMaxLen(requestFields, requestUnion);

        public int GetMaxBitLengthResponse() => computeMaxLen(responseFields, responseUnion);

        public int GetMinBitLengthRequest() => computeMinLen(requestFields, requestUnion);

        public int GetMinBitLengthResponse() => computeMinLen(responseFields, responseUnion);

        public override int GetMinBitLength() => GetMinBitLengthRequest();

        public override int GetMaxBitLength() => GetMaxBitLengthRequest();
    }
}
