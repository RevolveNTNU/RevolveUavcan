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
            parser = new DsdlParser(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestFiles", "TestDsdl"));
        }

        [TestMethod]
        [DeploymentItem("413.PitotTube.1.0.uavcan", "TestFiles/TestDsdl")]
        public void ParseValidDsdlMessageTest()
        {
            var source = File.ReadAllText(@"TestFiles/TestDsdl/413.PitotTube.1.0.uavcan");

            var result = parser.ParseSource("TestFiles/TestDsdl/413.PitotTube.1.0.uavcan", source);
            Assert.IsNotNull(result);

            // Verify full name
            Assert.AreEqual("PitotTube", result.FullName);

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

            Assert.AreEqual("saturated float32", pressureDeltaField.type.FullName);
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
            Assert.AreEqual("control.MzRefDebug", result.FullName);

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
                    Assert.AreEqual("control.PIDControl", field.type.FullName);
                    Assert.AreEqual(Category.COMPOUND, field.type.Category);
                    Assert.AreEqual(false, field.isConstant);
                    Assert.AreEqual(96, field.type.GetMaxBitLength());
                }
                else
                {
                    Assert.AreEqual("saturated float32", field.type.FullName);
                    Assert.AreEqual(Category.PRIMITIVE, field.type.Category);
                    Assert.AreEqual(false, field.isConstant);
                    Assert.AreEqual(32, field.type.GetMaxBitLength());
                }
            }

            Assert.AreEqual(1, parser.ParsedDsdlDict.Count);

            Assert.IsTrue(parser.ParsedDsdlDict.TryGetValue("control.PIDControl", out var pidControl));

            Assert.AreEqual(3, pidControl.RequestFields.Count);
            Assert.AreEqual(0, pidControl.ResponseFields.Count);

            var pidFields = new List<string> { "p_term", "i_term", "d_term" };
            foreach (var field in pidControl.RequestFields)
            {
                Assert.IsTrue(pidFields.Contains(field.name));
            }
        }

        [TestMethod]
        [DeploymentItem("12.DataMessage.1.0.uavcan", "TestFiles/TestDsdl/padding")]
        [DeploymentItem("DataType.1.0.uavcan", "TestFiles/TestDsdl/padding")]
        public void ParseValidDsdlMessageWithPaddingTest()
        {
            var source = File.ReadAllText(@"TestFiles/TestDsdl/padding/12.DataMessage.1.0.uavcan");

            var result = parser.ParseSource(@"TestFiles/TestDsdl/padding/12.DataMessage.1.0.uavcan", source);
            Assert.IsNotNull(result);

            // Verify full name
            Assert.AreEqual("padding.DataMessage", result.FullName);

            // This message should contain the three fields, as well as void3 padding before and after the compound type:
            // first_field padding second_field.data padding third_field

            // Verify fields and constants
            Assert.AreEqual(5, result.RequestFields.Count);
            Assert.AreEqual(0, result.ResponseFields.Count);
            Assert.AreEqual(0, result.RequestConstants.Count);
            Assert.AreEqual(0, result.ResponseConstants.Count);

            Assert.AreEqual("first_field", result.RequestFields[0].name);
            Assert.AreEqual("saturated uint5 first_field", result.RequestFields[0].ToString());
            Assert.AreEqual(Category.VOID, result.RequestFields[1].type.Category);
            Assert.AreEqual("void3", result.RequestFields[1].ToString());
            Assert.AreEqual(3, result.RequestFields[1].type.GetMaxBitLength());
            Assert.AreEqual("second_field", result.RequestFields[2].name);
            Assert.AreEqual(Category.VOID, result.RequestFields[3].type.Category);
            Assert.AreEqual(3, result.RequestFields[3].type.GetMaxBitLength());
            Assert.AreEqual("third_field", result.RequestFields[4].name);
        }


        [TestMethod]
        [DeploymentItem("35.RTDS.1.0.uavcan", "TestFiles/TestDsdl/dashboard")]
        public void ParseValidDsdlServiceTest()
        {
            var source = File.ReadAllText(@"TestFiles/TestDsdl/dashboard/35.RTDS.1.0.uavcan");

            var result = parser.ParseSource(@"TestFiles/TestDsdl/dashboard/35.RTDS.1.0.uavcan", source);
            Assert.IsNotNull(result);

            // Verify full name
            Assert.AreEqual("dashboard.RTDS", result.FullName);

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

            Assert.AreEqual("saturated uint8", command.type.FullName);
            Assert.AreEqual(Category.PRIMITIVE, command.type.Category);
            Assert.AreEqual(false, command.isConstant);
            Assert.AreEqual(8, command.type.GetMaxBitLength());

            var success = result.ResponseFields[0];

            Assert.AreEqual("saturated uint8", success.type.FullName);
            Assert.AreEqual(Category.PRIMITIVE, success.type.Category);
            Assert.AreEqual(false, success.isConstant);
            Assert.AreEqual(8, success.type.GetMaxBitLength());
        }

        [TestMethod]
        [DeploymentItem("seis.1.0.uavcan", "TestFiles/TestDsdl/common")]
        public void ParseValidConstantsTest()
        {
            var source = File.ReadAllText(@"TestFiles/TestDsdl/common/seis.1.0.uavcan");

            var result = parser.ParseSource(@"TestFiles/TestDsdl/common/seis.1.0.uavcan", source);
            Assert.IsNotNull(result);

            // Verify full name
            Assert.AreEqual("common.seis", result.FullName);

            // Verify constants
            Assert.AreEqual(7, result.RequestConstants.Count);

            Assert.AreEqual("a", result.RequestConstants[0].name);
            Assert.AreEqual("10", result.RequestConstants[0].StringValue);
            Assert.AreEqual("b", result.RequestConstants[1].name);
            Assert.AreEqual("1", result.RequestConstants[1].StringValue);
            Assert.AreEqual("c", result.RequestConstants[2].name);
            Assert.AreEqual("2", result.RequestConstants[2].StringValue);
            Assert.AreEqual("d", result.RequestConstants[3].name);
            Assert.AreEqual("3", result.RequestConstants[3].StringValue);
            Assert.AreEqual("e", result.RequestConstants[4].name);
            Assert.AreEqual(128.16.ToString(), result.RequestConstants[4].StringValue);
            Assert.AreEqual("f", result.RequestConstants[5].name);
            Assert.AreEqual("False", result.RequestConstants[5].StringValue);
            Assert.AreEqual("g", result.RequestConstants[6].name);
            Assert.AreEqual("L", result.RequestConstants[6].StringValue); // L in ASCII
        }

        [TestMethod]
        [DeploymentItem("413.PitotTube.1.0.uavcan", "TestFiles/TestDsdl")]
        [DeploymentItem("35.RTDS.1.0.uavcan", "TestFiles/TestDsdl/dashboard")]
        [DeploymentItem("136.MzRefDebug.1.0.uavcan", "TestFiles/TestDsdl/control")]
        [DeploymentItem("PIDControl.1.0.uavcan", "TestFiles/TestDsdl/control")]
        [DeploymentItem("60.cinco.1.0.uavcan", "TestFiles/TestDsdl/common")]
        [DeploymentItem("seis.1.0.uavcan", "TestFiles/TestDsdl/common")]
        [DeploymentItem("DataType.1.0.uavcan", "TestFiles/TestDsdl/data_messages")]
        [DeploymentItem("12.DataMessage.1.0.uavcan", "TestFiles/TestDsdl/data_messages")]
        public void ParseFullNamespaceTest()
        {
            parser.ParseAllDirectories();
            List<string> dsdlNames = new List<string> { "PitotTube", "dashboard.RTDS", "control.MzRefDebug", "control.PIDControl", "common.cinco", "common.seis", "padding.DataType", "padding.DataMessage" };

            Assert.AreEqual(dsdlNames.Count, parser.ParsedDsdlDict.Count);

            foreach (var keyValPair in parser.ParsedDsdlDict)
            {
                Assert.IsTrue(dsdlNames.Contains(keyValPair.Key));
            }
        }

        [TestMethod]
        public void ThrowDsdlExceptionOnUnknownDsdlPath()
        {
            var dsdlParser = new DsdlParser("");
            Assert.ThrowsException<DsdlException>(() => dsdlParser.ParseAllDirectories());
        }

        [TestMethod]
        [DataRow("int 8 command", "413.PitotTube.1.0.uavcan", DisplayName = "Invalid datatype syntax")]
        [DataRow("PIDControl speed", "413.PitotTube.1.0.uavcan", DisplayName = "Unknown Compound Type")]
        [DataRow("Int8 command", "413.PitotTube.1.0.uavcan", DisplayName = "Invalid datatype syntax")]
        [DataRow("int8 command\n---\nint8 command2\n---\nint8 command3", "413.PitotTube.1.0.uavcan", DisplayName = "Two service seperators")]
        [DataRow("int8 command\nuint8 command", "413.PitotTube.1.0.uavcan", DisplayName = "Duplicate field name")]
        [DataRow("int8 command", "413.PitotTube.1.0", DisplayName = "Invalid filename")]
        [DataRow("int8", "413.PitotTube.1.0", DisplayName = "Invalid syntax")]
        [DataRow("int8 command command", "413.PitotTube.1.0", DisplayName = "Invalid syntax")]
        public void ThrowDsdlExceptionTest(string dsdlRow, string filename)
        {
            var path = $"{Path.Join("TestFiles", "TestDsdl", filename)}";
            Assert.ThrowsException<DsdlException>(() => parser.ParseSource(path, dsdlRow));
        }

        [TestMethod]
        [DataRow(@"TestFiles/FaultyDsdl/12.bar.A.2.uavcan", DisplayName = "Invalid version syntax")]
        [DataRow(@"TestFiles/FaultyDsdl/12,2.foo.1.0.uavcan", DisplayName = "Invalid subject Id")]
        public void ThrowDsdlExceptionOnInvalidFilenameFormat(string filename)
        {
            var dsdlParser = new DsdlParser((Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestFiles")));

            var source = File.ReadAllText(filename);

            Assert.ThrowsException<DsdlException>(() => parser.ParseSource(filename, source));
        }
    }
}