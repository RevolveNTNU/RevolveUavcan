using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using RevolveUavcan.Tools;
using System;

namespace RevolveUavcanTest.Tools
{
    [TestClass]
    public class BitArrayToolsTests
    {
        [TestInitialize]
        public void Setup()
        {
            // No setup required for these tests
        }


        [DataTestMethod]
        [DynamicData(nameof(GetRangeValidData), DynamicDataSourceType.Method)]
        public void GetRangeValidArgsTest(BitArray bitArray, int startIndex, int numberOfBits, BitArray expectedResult)
        {
            var result = bitArray.GetRange(startIndex, numberOfBits);
            CollectionAssert.AreEqual(result, expectedResult);
        }

        public static IEnumerable<object[]> GetRangeValidData()
        {
            bool[] bools = { true, false, true, false, true, false, true, false, true, false, true, false };
            var bits = new BitArray(bools);

            yield return new object[] { bits, 0, 5, new BitArray(new bool[] { true, false, true, false, true }) };
            yield return new object[] { bits, 1, 5, new BitArray(new bool[] { false, true, false, true, false }) };
            yield return new object[] { bits, 2, 3, new BitArray(new bool[] { true, false, true }) };
            yield return new object[] { bits, 0, 1, new BitArray(new bool[] { true }) };
        }

        [DataTestMethod]
        [DynamicData(nameof(GetRangeInvalidData), DynamicDataSourceType.Method)]
        public void GetRangeInvalidArgsTest(BitArray bitArray, int startIndex, int numberOfBits)
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => bitArray.GetRange(startIndex, numberOfBits));
        }

        public static IEnumerable<object[]> GetRangeInvalidData()
        {
            bool[] bools = { true, false, true, false, true, false, true, false, true, false, true, false };
            var bits = new BitArray(bools);

            yield return new object[] { bits, 0, 13 };
            yield return new object[] { bits, 13, 5 };
            yield return new object[] { bits, 10, 3 };
            yield return new object[] { bits, 1, 12 };
        }
    }
}
