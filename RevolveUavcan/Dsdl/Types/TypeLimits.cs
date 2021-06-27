using System;

namespace RevolveUavcan.Dsdl.Types
{
    public class TypeLimits
    {

        /// <summary>
        /// Helper method for the DSDL parser. Returns the range that a generic datatype typeX can have. Used
        /// for the UAVCAN datatypes like uint9 and int23, which aren't native types
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="bitLength"></param>
        /// <returns></returns>
        /// <exception cref="DsdlException"></exception>
        public static Tuple<double, double> getRange(BaseType baseType, int bitLength)
        {
            switch (baseType)
            {
                case BaseType.UNSIGNED_INT:
                    return UnsignedIntRange(bitLength);
                case BaseType.SIGNED_INT:
                    return SignedIntRange(bitLength);
                case BaseType.FLOAT:
                    return FloatRange(bitLength);
                case BaseType.BOOLEAN:
                    return new Tuple<double, double>(0, 1);
            }
            throw new DsdlException("Cannot find range for unknown datatype");
        }

        /// <summary>
        /// Return the value a unsigned uint with bitlength size can contain
        /// </summary>
        /// <param name="bitLength"></param>
        /// <returns></returns>
        /// <exception cref="DsdlException"></exception>
        private static Tuple<double, double> UnsignedIntRange(int bitLength)
        {
            if (bitLength > 64 || bitLength <= 1)
            {
                throw new DsdlException("Bitsize out of range [1, 64]");
            }
            return new Tuple<double, double>(1, (1 << bitLength) - 1);
        }

        /// <summary>
        /// Return the value a unsigned int with bitlength size can contain
        /// </summary>
        /// <param name="bitLength"></param>
        /// <returns></returns>
        private static Tuple<double, double> SignedIntRange(int bitLength)
        {
            var unsignedRange = UnsignedIntRange(bitLength);
            return new Tuple<double, double>(-(unsignedRange.Item1 / 2) - 1, unsignedRange.Item2 / 2);
        }

        /// <summary>
        /// Return the value a float with bitlength size can contain. Only 16, 32 and 64 are supported
        /// </summary>
        /// <param name="bitLength"></param>
        /// <returns></returns>
        /// <exception cref="DsdlException"></exception>
        private static Tuple<double, double> FloatRange(int bitLength)
        {
            double max = 0;
            switch (bitLength)
            {
                case 16:
                    max = 65504;
                    break;
                case 32:
                    max = float.MaxValue;
                    break;
                case 64:
                    max = double.MaxValue;
                    break;
                default:
                    throw new DsdlException("Bitsize out of range [16, 32, 64]");
            }
            return new Tuple<double, double>(-max, max);
        }
    }
}
