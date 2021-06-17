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
            parser = new DsdlParser(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestFiles"));
        }

        [TestMethod]
        [DeploymentItem("413.PitotTube.1.0.uavcan", "TestFiles/TestDsdl")]
        public void ParseValidDsdlMessageTest()
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
        [DeploymentItem("136.MzRefDebug.1.0.uavcan", "TestFiles/TestDsdl/control")]
        [DeploymentItem("PIDControl.1.0.uavcan", "TestFiles/TestDsdl/control")]
        public void ParseValidDsdlMessageWithCompoundReferenceTest()
        {
            var source = File.ReadAllText(@"TestFiles/TestDsdl/control/136.MzRefDebug.1.0.uavcan");

            var result = parser.ParseSource(@"TestFiles/TestDsdl/control/136.MzRefDebug.1.0.uavcan", source);
            Assert.IsNotNull(result);

            // Verify full name
            Assert.AreEqual("TestDsdl.control.MzRefDebug", result.FullName);

            // Verify fields and constants
            Assert.AreEqual(3, result.requestFields.Count);
            Assert.AreEqual(0, result.responseFields.Count);
            Assert.AreEqual(0, result.requestConstants.Count);
            Assert.AreEqual(0, result.responseConstants.Count);

            Assert.AreEqual("closed_loop_pid", result.requestFields[0].name);
            Assert.AreEqual("closed_loop_yaw_ref", result.requestFields[1].name);
            Assert.AreEqual("open_loop_pid", result.requestFields[2].name);

            foreach (var field in result.requestFields)
            {
                if (field.name != "closed_loop_yaw_ref")
                {
                    Assert.AreEqual("TestDsdl.control.PIDControl", field.type.FullName);
                    Assert.AreEqual(Category.COMPOUND, field.type.Category);
                    Assert.AreEqual(false, field.isConstant);
                    Assert.AreEqual(96, field.type.GetMaxBitLength());
                }
                else
                {
                    Assert.AreEqual("saturated float", field.type.FullName);
                    Assert.AreEqual(Category.PRIMITIVE, field.type.Category);
                    Assert.AreEqual(false, field.isConstant);
                    Assert.AreEqual(32, field.type.GetMaxBitLength());
                }
            }

            Assert.AreEqual(1, parser.ParsedDsdlDict.Count);

            Assert.IsTrue(parser.ParsedDsdlDict.TryGetValue("TestDsdl.control.PIDControl", out var pidControl));

            Assert.AreEqual(3, pidControl.requestFields.Count);
            Assert.AreEqual(0, pidControl.responseFields.Count);

            var pidFields = new List<string> { "p_term", "i_term", "d_term" };
            foreach (var field in pidControl.requestFields)
            {
                Assert.IsTrue(pidFields.Contains(field.name));
            }
        }

        [TestMethod]
        [DeploymentItem("35.RTDS.1.0.uavcan", "TestFiles/TestDsdl/dashboard")]
        public void ParseValidDsdlServiceTest()
        {
            var source = File.ReadAllText(@"TestFiles/TestDsdl/dashboard/35.RTDS.1.0.uavcan");

            var result = parser.ParseSource(@"TestFiles/TestDsdl/dashboard/35.RTDS.1.0.uavcan", source);
            Assert.IsNotNull(result);

            // Verify full name
            Assert.AreEqual("TestDsdl.dashboard.RTDS", result.FullName);

            // Verify fields and constants
            Assert.AreEqual(1, result.requestFields.Count);
            Assert.AreEqual(1, result.responseFields.Count);
            Assert.AreEqual(2, result.requestConstants.Count);
            Assert.AreEqual(0, result.responseConstants.Count);

            Assert.AreEqual("command", result.requestFields[0].name);
            Assert.AreEqual("success", result.responseFields[0].name);

            Assert.AreEqual("PLAY", result.requestConstants[0].name);
            Assert.AreEqual("0", result.requestConstants[0].StringValue);
            Assert.AreEqual("FINISHED", result.requestConstants[1].name);
            Assert.AreEqual("1", result.requestConstants[1].StringValue);

            var command = result.requestFields[0];

            Assert.AreEqual("saturated uint", command.type.FullName);
            Assert.AreEqual(Category.PRIMITIVE, command.type.Category);
            Assert.AreEqual(false, command.isConstant);
            Assert.AreEqual(8, command.type.GetMaxBitLength());

            var success = result.responseFields[0];

            Assert.AreEqual("saturated uint", success.type.FullName);
            Assert.AreEqual(Category.PRIMITIVE, success.type.Category);
            Assert.AreEqual(false, success.isConstant);
            Assert.AreEqual(8, success.type.GetMaxBitLength());
        }

        [TestMethod]
        [DeploymentItem("413.PitotTube.1.0.uavcan", "TestFiles/TestDsdl")]
        [DeploymentItem("35.RTDS.1.0.uavcan", "TestFiles/TestDsdl/dashboard")]
        [DeploymentItem("136.MzRefDebug.1.0.uavcan", "TestFiles/TestDsdl/control")]
        [DeploymentItem("PIDControl.1.0.uavcan", "TestFiles/TestDsdl/control")]
        public void ParseFullNamespaceTest()
        {
            var parsedDsdl = parser.ParseAllDirectories();

            Assert.AreEqual(4, parsedDsdl.Count);

            List<string> dsdlNames = new List<string> { "TestDsdl.PitotTube", "TestDsdl.dashboard.RTDS", "TestDsdl.control.MzRefDebug", "TestDsdl.control.PIDControl" };
            foreach (var keyValPair in parsedDsdl)
            {
                Assert.IsTrue(dsdlNames.Contains(keyValPair.Key));
            }
        }
    }
}