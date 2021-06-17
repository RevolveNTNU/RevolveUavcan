using System;

namespace RevolveUavcan.Dsdl.Types
{
    public enum ArrayMode
    {
        STATIC,
        DYNAMIC
    }

    public class ArrayType : DsdlType
    {
        public DsdlType dataType;
        public ArrayMode mode;
        public int maxSize;

        public ArrayType(DsdlType dataType, ArrayMode mode, int maxSize) : base(
                        GetNormalizedDefinition(dataType, mode, maxSize), Category.ARRAY)
        {
            this.dataType = dataType;
            this.mode = mode;
            this.maxSize = maxSize;
        }

        public override int GetMaxBitLength()
        {
            var maxPayloadLength = maxSize * dataType.GetMaxBitLength();
            // For a dynamic array, we have to include the datalength field in the package
            return mode == ArrayMode.STATIC ? maxPayloadLength : maxPayloadLength + Convert.ToString(maxSize, 2).Length;
        }

        public override int GetMinBitLength()
        {
            if (mode == ArrayMode.STATIC)
            {
                return dataType.GetMinBitLength();
            }

            // Dynamic arrays can have 0 length
            return 0;
        }

        public override string ToString() => GetNormalizedDefinition(dataType, mode, maxSize);

        public static string GetNormalizedDefinition(DsdlType dataType, ArrayMode mode, int maxSize)
        {
            var baseString = dataType.FullName.ToString();
            var arrayString = mode == ArrayMode.STATIC
                            ? $"[{maxSize}]"
                            : $"[<={maxSize}]";
            return baseString + arrayString;
        }
    }
}
