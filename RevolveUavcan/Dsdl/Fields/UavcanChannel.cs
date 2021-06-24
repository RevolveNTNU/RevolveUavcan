using System;
using RevolveUavcan.Dsdl.Types;

namespace RevolveUavcan.Dsdl.Fields
{
    public class UavcanChannel
    {
        public BaseType Basetype { get; set; }
        public int Size { get; set; }
        public string FieldName { get; set; }
        public int ArraySize { get; set; }
        public bool IsDynamicArray { get; set; }
        public int NumberOfBitsInSize { get; set; }


        public UavcanChannel(BaseType basetype, int size, string fieldName)
        {
            Basetype = basetype;
            Size = size;
            FieldName = fieldName;
            ArraySize = 0;
            IsDynamicArray = false;
        }

        public UavcanChannel(BaseType basetype, int size, string fieldName, int arraySize, bool isDynamicArray)
        {
            Basetype = basetype;
            Size = size;
            FieldName = fieldName;
            ArraySize = arraySize;
            IsDynamicArray = isDynamicArray;

            if (IsDynamicArray)
            {
                var temp = Math.Log(arraySize, 2);
                NumberOfBitsInSize = Convert.ToInt32(Math.Ceiling(temp));
            }
        }
    }
}
