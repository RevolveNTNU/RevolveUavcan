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
            Assert.AreEqual(2, result.RequestFields.Count);
            Assert.AreEqual(0, result.ResponseFields.Count);
            Assert.AreEqual(2, result.RequestConstants.Count);
            Assert.AreEqual(0, result.ResponseConstants.Count);

            Assert.AreEqual("pressure_delta", result.RequestFields[0].name);
            Assert.AreEqual("error_bf", result.RequestFields[1].name);

            Assert.AreEqual("ERROR_PRESSURE_DELTA_OFFSET", result.RequestConstants[0].name);
            Assert.AreEqual("0", result.RequestConstants[0].StringValue);
            Assert.AreEqual("ERROR_BITMASK", result.RequestConstants[1].name);
            Assert.AreEqual("15", result.RequestConstants[1].StringValue);

            var pressureDeltaField = result.RequestFields[0];

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
            Assert.AreEqual(3, result.RequestFields.Count);
            Assert.AreEqual(0, result.ResponseFields.Count);
            Assert.AreEqual(0, result.RequestConstants.Count);
            Assert.AreEqual(0, result.ResponseConstants.Count);

            Assert.AreEqual("closed_loop_pid", result.RequestFields[0].name);
            Assert.AreEqual("closed_loop_yaw_ref", result.RequestFields[1].name);
            Assert.AreEqual("open_loop_pid", result.RequestFields[2].name);

            foreach (var field in result.RequestFields)
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

            Assert.AreEqual(3, pidControl.RequestFields.Count);
            Assert.AreEqual(0, pidControl.ResponseFields.Count);

            var pidFields = new List<string> { "p_term", "i_term", "d_term" };
            foreach (var field in pidControl.RequestFields)
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
            Assert.AreEqual(1, result.RequestFields.Count);
            Assert.AreEqual(1, result.ResponseFields.Count);
            Assert.AreEqual(2, result.RequestConstants.Count);
            Assert.AreEqual(0, result.ResponseConstants.Count);

            Assert.AreEqual("command", result.RequestFields[0].name);
            Assert.AreEqual("success", result.ResponseFields[0].name);

            Assert.AreEqual("PLAY", result.RequestConstants[0].name);
            Assert.AreEqual("0", result.RequestConstants[0].StringValue);
            Assert.AreEqual("FINISHED", result.RequestConstants[1].name);
            Assert.AreEqual("1", result.RequestConstants[1].StringValue);

            var command = result.RequestFields[0];

            Assert.AreEqual("saturated uint", command.type.FullName);
            Assert.AreEqual(Category.PRIMITIVE, command.type.Category);
            Assert.AreEqual(false, command.isConstant);
            Assert.AreEqual(8, command.type.GetMaxBitLength());

            var success = result.ResponseFields[0];

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
        [DeploymentItem("60.cinco.1.0.uavcan", "TestFiles/TestDsdl/common")]
        public void ParseFullNamespaceTest()
        {
            parser.ParseAllDirectories();

            Assert.AreEqual(5, parser.ParsedDsdlDict.Count);

            List<string> dsdlNames = new List<string> { "TestDsdl.PitotTube", "TestDsdl.dashboard.RTDS", "TestDsdl.control.MzRefDebug", "TestDsdl.control.PIDControl", "TestDsdl.common.cinco" };
            foreach (var keyValPair in parser.ParsedDsdlDict)
            {
                Assert.IsTrue(dsdlNames.Contains(keyValPair.Key));
            }
        }
    }
}