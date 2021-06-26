using System;
using System.Collections.Generic;
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

        //public UavcanChannel(BaseType basetype, int size, string fieldName, int arraySize, bool isDynamicArray)
        //{
        //    Basetype = basetype;
        //    Size = size;
        //    FieldName = fieldName;
        //    ArraySize = arraySize;
        //    IsDynamicArray = isDynamicArray;

        //    if (IsDynamicArray)
        //    {
        //        var temp = Math.Log(arraySize, 2);
        //        NumberOfBitsInSize = Convert.ToInt32(Math.Ceiling(temp));
        //    }
        //}

        public override bool Equals(object obj)
        {
            return obj is UavcanChannel channel &&
                   Basetype == channel.Basetype &&
                   Size == channel.Size &&
                   FieldName == channel.FieldName &&
                   ArraySize == channel.ArraySize &&
                   IsDynamicArray == channel.IsDynamicArray &&
                   NumberOfBitsInSize == channel.NumberOfBitsInSize;
        }

        public override int GetHashCode()
        {
            int hashCode = 820166189;
            hashCode = hashCode * -1521134295 + Basetype.GetHashCode();
            hashCode = hashCode * -1521134295 + Size.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FieldName);
            hashCode = hashCode * -1521134295 + ArraySize.GetHashCode();
            hashCode = hashCode * -1521134295 + IsDynamicArray.GetHashCode();
            hashCode = hashCode * -1521134295 + NumberOfBitsInSize.GetHashCode();
            return hashCode;
        }
    }
}
