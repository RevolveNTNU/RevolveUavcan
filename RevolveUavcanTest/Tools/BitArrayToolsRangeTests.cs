using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using RevolveUavcan.Tools;
using System;

namespace RevolveUavcanTest.Tools
{
    [TestClass]
    public class BitArrayToolsRangeTests
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
            yield return new object[] { bits, -1, 10 };
        }

        [DataTestMethod]
        [DynamicData(nameof(InsertRangeValidData), DynamicDataSourceType.Method)]
        public void InsertRangeValidArgsTest(BitArray sourceArray, BitArray targetArray, int startIndex, BitArray expectedResult)
        {
            var result = sourceArray.InsertRange(targetArray, startIndex);
            CollectionAssert.AreEqual(result, expectedResult);
        }

        public static IEnumerable<object[]> InsertRangeValidData()
        {
            bool[] bools = { false, false, false, false, false, false };

            yield return new object[] { new BitArray(bools),
                new BitArray(new bool[] { true, false, true, false, true }), 0,
                new BitArray(new bool[] { true, false, true, false, true, false }) };

            yield return new object[] { new BitArray(bools),
                new BitArray(new bool[] {        false, true, false, true, false }), 1,
                new BitArray(new bool[] { false, false, true, false, true, false }) };

            yield return new object[] { new BitArray(bools),
                new BitArray(new bool[] {               true, false, true }), 2,
                new BitArray(new bool[] { false, false, true, false, true, false }) };

            yield return new object[] { new BitArray(bools),
                new BitArray(new bool[] {               true }), 2,
                new BitArray(new bool[] { false, false, true, false, false, false }) };
        }

        [DataTestMethod]
        [DynamicData(nameof(InsertRangeInvalidData), DynamicDataSourceType.Method)]
        public void InsertRangeInvalidArgsTest(BitArray bitArray, BitArray targetArray, int startIndex)
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => bitArray.InsertRange(targetArray, startIndex));
        }

        public static IEnumerable<object[]> InsertRangeInvalidData()
        {
            bool[] bools = { true, false, true, false, true, false, true, false, true, false, true, false };
            bool[] lessBools = { true, false, true, false, true };
            yield return new object[] { new BitArray(bools), new BitArray(bools), 1 };
            yield return new object[] { new BitArray(bools), new BitArray(lessBools), 8 };
            yield return new object[] { new BitArray(bools), new BitArray(lessBools), -1 };
            yield return new object[] { new BitArray(bools), new BitArray(bools), 15 };
        }
    }
}
