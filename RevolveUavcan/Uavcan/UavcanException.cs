using System;

namespace RevolveUavcan.Uavcan
{
    public class UavcanException : Exception
    {
        public UavcanException()
        {
        }
        public UavcanException(string message) : base(message)
        {
        }
        public UavcanException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
