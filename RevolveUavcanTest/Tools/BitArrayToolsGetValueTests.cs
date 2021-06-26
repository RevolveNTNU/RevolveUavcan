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
            Assert.AreEqual(expectedResult, result);
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
            Assert.AreEqual(expectedResult, result);
        }

        public static IEnumerable<object[]> GetUIntValidData()
        {
            yield return new object[] { new BitArray(BitConverter.GetBytes(12)), 12 };
            yield return new object[] { new BitArray(BitConverter.GetBytes(128)), 128 };
            yield return new object[] { new BitArray(BitConverter.GetBytes(17)), 17 };
            yield return new object[] { new BitArray(BitConverter.GetBytes(17)), 17 };
            yield return new object[] { new BitArray(BitConverter.GetBytes(12_000)), 12_000 };
        }


        [DataTestMethod]
        [DynamicData(nameof(GetUIntInvalidData), DynamicDataSourceType.Method)]
        public void GetUIntInvalidArgsTest(BitArray bitArray)
        {
            Assert.ThrowsException<ArgumentException>(() => bitArray.GetIntFromBitArray());
        }

        public static IEnumerable<object[]> GetUIntInvalidData()
        {
            yield return new object[] { new BitArray(64) };
        }

        [DataTestMethod]
        [DynamicData(nameof(GetLongValidData), DynamicDataSourceType.Method)]
        public void GetLongValidArgsTest(BitArray bitArray, long expectedResult)
        {
            var result = bitArray.GetLongFromBitArray();
            Assert.AreEqual(expectedResult, result);
        }

        public static IEnumerable<object[]> GetLongValidData()
        {
            yield return new object[] { BitArrayTools.GetBitArrayFromLong(-16, 64), -16 };
            yield return new object[] { new BitArray(BitConverter.GetBytes((long)128)), 128 };
            yield return new object[] { new BitArray(BitConverter.GetBytes((long)12_000_000)), 12_000_000 };
        }


        [DataTestMethod]
        [DynamicData(nameof(GetLongInvalidData), DynamicDataSourceType.Method)]
        public void GetLongInvalidArgsTest(BitArray bitArray)
        {
            Assert.ThrowsException<ArgumentException>(() => bitArray.GetLongFromBitArray());
        }

        public static IEnumerable<object[]> GetLongInvalidData()
        {
            yield return new object[] { new BitArray(128) };
        }

        [TestMethod]
        [DynamicData(nameof(GetUintInvalidData), DynamicDataSourceType.Method)]
        public void GetUintInvalidArgsTest(BitArray bitArray)
        {
            Assert.ThrowsException<ArgumentException>(() => bitArray.GetUIntFromBitArray());
        }

        public static IEnumerable<object[]> GetUintInvalidData()
        {
            yield return new object[] { new BitArray(69) };
        }


        [DataTestMethod]
        [DynamicData(nameof(GetFloatAndDoubleValidData), DynamicDataSourceType.Method)]
        public void GetFloatAndDoubleValidArgsTest(BitArray bitArray, double expectedResult)
        {
            var result = bitArray.GetFloatFromBitArray();
            Assert.AreEqual(expectedResult, result);
        }

        public static IEnumerable<object[]> GetFloatAndDoubleValidData()
        {
            yield return new object[] { new BitArray(BitConverter.GetBytes(128D)), 128D };
            yield return new object[] { new BitArray(BitConverter.GetBytes(-128D)), -128D };
            yield return new object[] { new BitArray(BitConverter.GetBytes(-128F)), -128F };
            yield return new object[] { new BitArray(BitConverter.GetBytes(128F)), 128F };
        }


        [TestMethod]
        [DynamicData(nameof(GetFloatAndDoubleInvalidData), DynamicDataSourceType.Method)]
        public void GetFloatAndDoubleInvalidArgsTest(BitArray bitArray, double expectedResult)
        {
            Assert.ThrowsException<ArgumentException>(() => bitArray.GetFloatFromBitArray());
        }

        public static IEnumerable<object[]> GetFloatAndDoubleInvalidData()
        {
            yield return new object[] { new BitArray(42) };
            yield return new object[] { new BitArray(69) };
        }
    }
}
