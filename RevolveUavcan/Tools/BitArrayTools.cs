using RevolveUavcan.Dsdl.Fields;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RevolveUavcan.Tools
{

    /// <summary>
    /// Decoding bitarrays assume the input to be little endian, encoding handles both.
    /// </summary>
    public static class BitArrayTools
    {
        /// <summary>
        /// Gets a range from a BitArray based on provided parameters.
        /// </summary>
        /// <param name="bitArray">The BitArray to be split</param>
        /// <param name="startIndex">The start index to start splitting</param>
        /// <param name="numberOfBits">The number of indices before ending splitting</param>
        /// <returns></returns>
        public static BitArray GetRange(this BitArray bitArray, int startIndex, int numberOfBits)
        {
            BitArray splitArray = new BitArray(numberOfBits);

            for (int i = startIndex; i < numberOfBits + startIndex; i++)
            {
                splitArray.Set(i - startIndex, bitArray.Get(i));
            }

            return splitArray;
        }

        public static BitArray InsertRange(this BitArray sourceArray, BitArray targetArray, int startIndex)
        {
            for (int i = 0; i < targetArray.Count; i++)
            {
                sourceArray.Set(startIndex + i, targetArray.Get(i));
            }

            return sourceArray;
        }

        /// <summary>
        /// Calculate the integer value of a BitArray
        /// </summary>
        /// <param name="bitArray">The BitArray we want to calculate the corresponding integer value for</param>
        /// <returns>An integer representation of the BitArray</returns>
        public static int GetIntFromBitArray(this BitArray bitArray)
        {
            if (bitArray.Length > 32)
            {
                throw new ArgumentException("BitArray length cannot be greater than 32 bits.");
            }

            // Initialize bitArrayWithMsb with values from bitArray and fill it with MSB
            BitArray bitArrayWithMsb = FillBitArrayWithMSB(bitArray, 32);

            // Find and return corresponding integer value from bitArrayWithMsb
            int[] array = new int[1];
            bitArrayWithMsb.CopyTo(array, 0);
            return array[0];
        }

        /// <summary>
        /// Calculate the uint value of a BitArray
        /// </summary>
        /// <param name="bitArray">The BitArray we want to calculate the corresponding uint value for</param>
        /// <returns>An uint representation of the BitArray</returns>
        public static uint GetUIntFromBitArray(this BitArray bitArray)
        {
            if (bitArray.Length > 32)
            {
                throw new ArgumentException("BitArray length cannot be greater than 64 bits.");
            }

            // Find and return corresponding integer value from bitArrayWithMsb
            uint[] array = new uint[1];
            bitArray.CopyTo(array, 0);
            return array[0];
        }

        public static BitArray FillBitArrayWithMSB(this BitArray bitArray, int finalSize)
        {
            // Get most significant bit from bitArray
            bool mostSignificantBit = bitArray.Get(bitArray.Length - 1);

            // Initialize template BitArray with set size and fill it with lastBit value
            BitArray template = new BitArray(finalSize, mostSignificantBit);

            // Iterate through bitArray and set its values in template
            for (int i = 0; i < bitArray.Length; i++)
            {
                template[i] = bitArray[i];
            }

            return template;
        }

        /// <summary>
        /// Calculate the long value of a BitArray
        /// </summary>
        /// <param name="bitArray">The BitArray we want to calculate the corresponding long value for</param>
        /// <returns>An integer representation of the BitArray</returns>
        public static long GetLongFromBitArray(this BitArray bitArray)
        {
            if (bitArray.Length > 64)
            {
                throw new ArgumentException("BitArray length cannot be greater than 64 bits.");
            }

            // Initialize bitArrayWithMsb with values from bitArray and fill it with MSB
            BitArray bitArrayWithMsb = FillBitArrayWithMSB(bitArray, 64);

            // Find and return corresponding integer value from bitArrayWithMsb
            int[] array = new int[2];
            bitArrayWithMsb.CopyTo(array, 0);
            return array[0] + ((long)array[1] << 32);
        }

        public static double GetFloatFromBitArray(this BitArray dataBits)
        {
            if (dataBits.Length != 64 && dataBits.Length != 32)
            {
                throw new ArgumentException("Invalid bit length.");
            }

            byte[] bytes = new byte[dataBits.Length / 8];

            dataBits.CopyTo(bytes, 0);

            return dataBits.Length == 64 ? BitConverter.ToDouble(bytes) : BitConverter.ToSingle(bytes);
        }

        public static byte[] GetByteArrayFromBitArray(this BitArray bitArray)
        {
            int byteLength = bitArray.Length / 8;

            if (bitArray.Length % 8 != 0)
            {
                byteLength += 1;
            }

            byte[] bytes = new byte[byteLength];

            bitArray.CopyTo(bytes, 0);

            return bytes;
        }

        public static BitArray GetBitArrayForUavcanChannels(List<UavcanChannel> uavcanChannels)
        {
            var bitLength = 0;

            foreach (UavcanChannel channel in uavcanChannels)
            {
                bitLength += channel.Size;
            }

            return new BitArray(bitLength);
        }

        public static BitArray GetBitArrayFromLong(long value, int size)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            BitArray bitArray = new BitArray(bytes);

            return GetRange(bitArray, 0, size);
        }

        public static BitArray GetBitArrayFromUInt(uint value, int size)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            BitArray bitArray = new BitArray(bytes);

            return GetRange(bitArray, 0, size);
        }


        public static BitArray GetBitArrayFromDouble(double value, int size)
        {
            var bytes = size == 64 ? BitConverter.GetBytes(value) : BitConverter.GetBytes((float)value);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return new BitArray(bytes);
        }

    }
}
