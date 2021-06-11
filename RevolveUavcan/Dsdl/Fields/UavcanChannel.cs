using System;
using RevolveUavcan.Dsdl.Types;

namespace RevolveUavcan.Dsdl.Fields
{
    public class UavcanChannel
    {
        public BaseType Basetype { get; set; }
        public int Size { get; set; }
        public string FieldName { get; set; }

        public bool IsArray { get; set; }
        public int ArraySize { get; set; }
        public bool IsDynamic { get; set; }
        public int NumberOfBitsInSize { get; set; }


        public UavcanChannel(BaseType basetype, int size, string fieldName)
        {
            this.Basetype = basetype;
            this.Size = size;
            this.FieldName = fieldName;
            IsArray = false;
            ArraySize = 0;
            IsDynamic = false;
        }

        public UavcanChannel(BaseType basetype, int size, string fieldName, bool isArray, int arraySize, bool isDynamic)
        {
            this.Basetype = basetype;
            this.Size = size;
            this.FieldName = fieldName;
            this.IsArray = isArray;
            this.ArraySize = arraySize;
            this.IsDynamic = isDynamic;

            if (isDynamic)
            {
                var temp = Math.Log(arraySize, 2);
                NumberOfBitsInSize = Convert.ToInt32(Math.Ceiling(temp));
            }
        }
    }
}
