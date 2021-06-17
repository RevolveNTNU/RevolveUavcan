﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolveUavcan.Dsdl;
using RevolveUavcan.Dsdl.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace RevolveUavcanTest.Dsdl
{
    [TestClass]
    public class DsdlParserTests
    {
        private DsdlParser parser;

        [TestInitialize]
        public void Setup()
        {
            parser = new DsdlParser(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestFiles"));
        }

        [TestMethod]
        [DeploymentItem("413.PitotTube.1.0.uavcan", "TestFiles/TestDsdl")]
        public void ParseValidDsdlTest()
        {
            var source = File.ReadAllText(@"TestFiles/TestDsdl/413.PitotTube.1.0.uavcan");

            var result = parser.ParseSource("TestFiles/TestDsdl/413.PitotTube.1.0.uavcan", source);
            Assert.IsNotNull(result);

            // Verify full name
            Assert.AreEqual("TestDsdl.PitotTube", result.FullName);

            // Verify fields and constants
            Assert.AreEqual(2, result.requestFields.Count);
            Assert.AreEqual(0, result.responseFields.Count);
            Assert.AreEqual(2, result.requestConstants.Count);
            Assert.AreEqual(0, result.responseConstants.Count);

            Assert.AreEqual("pressure_delta", result.requestFields[0].name);
            Assert.AreEqual("error_bf", result.requestFields[1].name);

            Assert.AreEqual("ERROR_PRESSURE_DELTA_OFFSET", result.requestConstants[0].name);
            Assert.AreEqual("0", result.requestConstants[0].StringValue);
            Assert.AreEqual("ERROR_BITMASK", result.requestConstants[1].name);
            Assert.AreEqual("15", result.requestConstants[1].StringValue);

            var pressureDeltaField = result.requestFields[0];

            Assert.AreEqual("saturated float", pressureDeltaField.type.FullName);
            Assert.AreEqual(Category.PRIMITIVE, pressureDeltaField.type.Category);
            Assert.AreEqual(false, pressureDeltaField.isConstant);
            Assert.AreEqual(32, pressureDeltaField.type.GetMaxBitLength());
        }

        [TestMethod]
        [DeploymentItem("413.PitotTube.1.0.uavcan", "TestFiles/TestDsdl")]
        [DeploymentItem("35.RTDS.1.0.uavcan", "TestFiles/TestDsdl/dashboard")]
        public void ParseFullNamespaceTest()
        {
            var parsedDsdl = parser.ParseAllDirectories();

            Assert.AreEqual(2, parsedDsdl.Count);

            List<string> dsdlNames = new List<string> { "TestDsdl.PitotTube", "TestDsdl.dashboard.RTDS" };
            foreach (var keyValPair in parsedDsdl)
            {
                Assert.IsTrue(dsdlNames.Contains(keyValPair.Key));
            }
        }
    }
}