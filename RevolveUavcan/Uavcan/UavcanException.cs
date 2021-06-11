using System;

namespace RevolveUavcan.Uavcan
{
    public class UavcanException : Exception
    {
        public UavcanException(string message) : base(message)
        {
        }
    }
}
