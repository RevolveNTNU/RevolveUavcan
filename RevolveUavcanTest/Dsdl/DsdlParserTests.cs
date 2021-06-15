using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            parser = new DsdlParser(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));
        }

        [TestMethod]
        [DeploymentItem("413.PitotTube.1.0.uavcan", "TestFiles/TestDsdl")]
        public void ParseValidDsdlTest()
        {
            var source = File.ReadAllText(@"TestFiles/TestDsdl/413.PitotTube.1.0.uavcan");

            var result = parser.ParseSource("TestDsdl/413.PitotTube.1.0.uavcan", source);
            Assert.IsNotNull(result);

            // Verify full name
            Assert.AreEqual("TestDsdl.PitotTube", result.fullName);

            // Verify fields and constants
            Assert.AreEqual(2, result.requestFields.Count);
            Assert.AreEqual(0, result.responseFields.Count);
            Assert.AreEqual(2, result.requestConstants.Count);
            Assert.AreEqual(0, result.responseConstants.Count);

            Assert.AreEqual("pressure_delta", result.requestFields[0].name);
            Assert.AreEqual("error_bf", result.requestFields[1].name);

            Assert.AreEqual("ERROR_PRESSURE_DELTA_OFFSET", result.requestConstants[0].name);
            Assert.AreEqual("ERROR_BITMASK", result.requestConstants[1].name);

            var pressureDeltaField = result.requestFields[0];

            Assert.AreEqual("saturated float", pressureDeltaField.type.fullName);
            Assert.AreEqual(Category.PRIMITIVE, pressureDeltaField.type.Category);
            Assert.AreEqual(false, pressureDeltaField.isConstant);
            Assert.AreEqual(32, pressureDeltaField.type.GetMaxBitLength());
        }
    }
}