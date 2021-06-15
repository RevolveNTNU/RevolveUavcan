using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using RevolveUavcan.Tools;
using System;

namespace RevolveUavcanTest.Tools
{
    [TestClass]
    public class BitArrayToolsGetValueTests
    {
        [TestInitialize]
        public void Setup()
        {
            // No setup required for these tests
        }


        [DataTestMethod]
        [DynamicData(nameof(GetIntValidData), DynamicDataSourceType.Method)]
        public void GetIntValidArgsTest(BitArray bitArray, int expectedResult)
        {
            var result = bitArray.GetIntFromBitArray();
            Assert.AreEqual(result, expectedResult);
        }

        public static IEnumerable<object[]> GetIntValidData()
        {
            yield return new object[] { new BitArray(BitConverter.GetBytes(12)), 12 };
            yield return new object[] { new BitArray(BitConverter.GetBytes(128)), 128 };
            yield return new object[] { new BitArray(BitConverter.GetBytes(-17)), -17 };
            yield return new object[] { new BitArray(BitConverter.GetBytes(12_000)), 12_000 };
        }


        [DataTestMethod]
        [DynamicData(nameof(GetIntInvalidData), DynamicDataSourceType.Method)]
        public void GetIntInvalidArgsTest(BitArray bitArray)
        {
            Assert.ThrowsException<ArgumentException>(() => bitArray.GetIntFromBitArray());
        }

        public static IEnumerable<object[]> GetIntInvalidData()
        {
            yield return new object[] { new BitArray(64) };
        }

        [DataTestMethod]
        [DynamicData(nameof(GetUIntValidData), DynamicDataSourceType.Method)]
        public void GetUIntValidArgsTest(BitArray bitArray, int expectedResult)
        {
            var result = bitArray.GetIntFromBitArray();
            Assert.AreEqual(result, expectedResult);
        }

        public static IEnumerable<object[]> GetUIntValidData()
        {
            yield return new object[] { new BitArray(BitConverter.GetBytes(12)), 12 };
            yield return new object[] { new BitArray(BitConverter.GetBytes(128)), 128 };
            yield return new object[] { new BitArray(BitConverter.GetBytes(17)), 17 };
            yield return new object[] { new BitArray(BitConverter.GetBytes(17)), 17 };
            yield return new object[] { new BitArray(BitConverter.GetBytes(12_000)), 12_001 };
        }


        [DataTestMethod]
        [DynamicData(nameof(GetIntInvalidData), DynamicDataSourceType.Method)]
        public void GetUIntInvalidArgsTest(BitArray bitArray)
        {
            Assert.ThrowsException<ArgumentException>(() => bitArray.GetIntFromBitArray());
        }

        public static IEnumerable<object[]> GetUIntInvalidData()
        {
            yield return new object[] { new BitArray(64) };
        }
    }
}
