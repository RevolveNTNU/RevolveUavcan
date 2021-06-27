using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolveUavcan.Dsdl;
using RevolveUavcan.Dsdl.Fields;
using RevolveUavcan.Dsdl.Interfaces;
using RevolveUavcan.Dsdl.Types;
using RevolveUavcan.Uavcan;
using System.Collections.Generic;

namespace RevolveUavcanTest.Uavcan
{
    [TestClass]
    public class UavcanSerializationRulesGeneratorTests
    {
        [DataTestMethod]
        [DynamicData(nameof(GetDsdlAndResult), DynamicDataSourceType.Method)]
        [DeploymentItem("60.cinco.1.0.uavcan", "TestFiles/TestDsdl/common")]
        public void GenerateSingleMessageSerializationRule(
            Dictionary<string, CompoundType> dsdlDict,
            uint expectedSubjectId,
            string expectedMessageName,
            List<UavcanChannel> expectedSerializationRule,
            uint wrongSubjectId,
            string wrongMessageName)
        {
            var dsdlParser = new Moq.Mock<IDsdlParser>();
            dsdlParser.Setup(d => d.ParsedDsdlDict).Returns(dsdlDict);
            var rulesGenerator = new UavcanSerializationRulesGenerator(dsdlParser.Object);

            try
            {
                rulesGenerator.Init();
            }
            catch (UavcanException e)
            {
                Assert.Fail("Expected no exception, but got: " + e.Message);
            }

            Assert.IsTrue(rulesGenerator.TryGetSerializationRuleForMessage(expectedSubjectId, out var idRules));
            Assert.IsTrue(rulesGenerator.TryGetSerializationRuleForMessage(expectedMessageName, out var nameRules));

            Assert.IsFalse(rulesGenerator.TryGetSerializationRuleForMessage(wrongSubjectId, out var _));
            Assert.IsFalse(rulesGenerator.TryGetSerializationRuleForMessage(wrongMessageName, out var _));

            CollectionAssert.AreEqual(expectedSerializationRule, idRules);
            CollectionAssert.AreEqual(expectedSerializationRule, nameRules);

        }

        public static IEnumerable<object[]> GetDsdlAndResult()
        {
            // Compound type that is not a message
            string fullNamePid = "TestDsdl.control.PIDControl";
            string wrongName = "TestDsdl.wrong.name";
            uint subjectIdPid = 0;
            uint wrongSubjectId = 42069;
            var compoundTypePid = new CompoundType(fullNamePid,
                MessageType.MESSAGE,
                subjectIdPid,
                new System.Tuple<int, int>(1, 0));

            compoundTypePid.RequestFields.Add(new Field(new PrimitiveType(BaseType.FLOAT, 32, CastMode.SATURATED), "p_term"));
            compoundTypePid.RequestFields.Add(new Field(new PrimitiveType(BaseType.FLOAT, 32, CastMode.SATURATED), "i_term"));
            compoundTypePid.RequestFields.Add(new Field(new PrimitiveType(BaseType.FLOAT, 32, CastMode.SATURATED), "d_term"));


            string fullNameCinco = "TestDsdl.common.cinco";
            uint subjectIdCinco = 60;
            CompoundType compoundTypeCinco = new CompoundType(fullNameCinco,
                MessageType.MESSAGE,
                subjectIdCinco,
                new System.Tuple<int, int>(1, 0)
            );

            compoundTypeCinco.RequestFields.Add(new Field(new PrimitiveType(BaseType.SIGNED_INT, 3, CastMode.SATURATED), "a"));
            compoundTypeCinco.RequestFields.Add(new Field(new PrimitiveType(BaseType.UNSIGNED_INT, 3, CastMode.SATURATED), "b"));
            compoundTypeCinco.RequestFields.Add(new Field(new PrimitiveType(BaseType.FLOAT, 32, CastMode.SATURATED), "c"));
            compoundTypeCinco.RequestFields.Add(new Field(new PrimitiveType(BaseType.FLOAT, 64, CastMode.SATURATED), "d"));
            compoundTypeCinco.RequestFields.Add(new Field(new VoidType(16), ""));
            compoundTypeCinco.RequestFields.Add(new Field(new PrimitiveType(BaseType.BOOLEAN, 1, CastMode.SATURATED), "f"));
            compoundTypeCinco.RequestFields.Add(new Field(new ArrayType(new PrimitiveType(BaseType.UNSIGNED_INT, 8, CastMode.SATURATED), ArrayMode.STATIC, 3), "g"));
            compoundTypeCinco.RequestFields.Add(new Field(new ArrayType(compoundTypePid, ArrayMode.STATIC, 2), "h"));

            List<UavcanChannel> expectedRuleCinco = new List<UavcanChannel>();
            expectedRuleCinco.Add(new UavcanChannel(BaseType.SIGNED_INT, 3, "TestDsdl.common.cinco.a"));
            expectedRuleCinco.Add(new UavcanChannel(BaseType.UNSIGNED_INT, 3, "TestDsdl.common.cinco.b"));
            expectedRuleCinco.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.common.cinco.c"));
            expectedRuleCinco.Add(new UavcanChannel(BaseType.FLOAT, 64, "TestDsdl.common.cinco.d"));
            expectedRuleCinco.Add(new UavcanChannel(BaseType.VOID, 16, ""));
            expectedRuleCinco.Add(new UavcanChannel(BaseType.BOOLEAN, 1, "TestDsdl.common.cinco.f"));
            expectedRuleCinco.Add(new UavcanChannel(BaseType.UNSIGNED_INT, 8, "TestDsdl.common.cinco.g_0"));
            expectedRuleCinco.Add(new UavcanChannel(BaseType.UNSIGNED_INT, 8, "TestDsdl.common.cinco.g_1"));
            expectedRuleCinco.Add(new UavcanChannel(BaseType.UNSIGNED_INT, 8, "TestDsdl.common.cinco.g_2"));
            expectedRuleCinco.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.common.cinco.h_0.p_term"));
            expectedRuleCinco.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.common.cinco.h_0.i_term"));
            expectedRuleCinco.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.common.cinco.h_0.d_term"));
            expectedRuleCinco.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.common.cinco.h_1.p_term"));
            expectedRuleCinco.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.common.cinco.h_1.i_term"));
            expectedRuleCinco.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.common.cinco.h_1.d_term"));

            yield return new object[] { new Dictionary<string, CompoundType> { { fullNameCinco, compoundTypeCinco } }, subjectIdCinco, fullNameCinco, expectedRuleCinco, wrongSubjectId, wrongName };

            // A compound type which has a compound type as field
            string fullNameMzRef = "TestDsdl.control.MzRefDebug";
            uint subjectIdMzRef = 136;
            var compoundTypeMzRef = new CompoundType(fullNameMzRef,
                MessageType.MESSAGE,
                subjectIdMzRef,
                new System.Tuple<int, int>(1, 0));

            compoundTypeMzRef.RequestFields.Add(new Field(compoundTypePid, "closed_loop_pid"));
            compoundTypeMzRef.RequestFields.Add(new Field(new PrimitiveType(BaseType.FLOAT, 32, CastMode.SATURATED), "closed_loop_yaw_ref"));
            compoundTypeMzRef.RequestFields.Add(new Field(compoundTypePid, "open_loop_pid"));


            var expectedRuleMzRef = new List<UavcanChannel>();
            expectedRuleMzRef.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.control.MzRefDebug.closed_loop_pid.p_term"));
            expectedRuleMzRef.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.control.MzRefDebug.closed_loop_pid.i_term"));
            expectedRuleMzRef.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.control.MzRefDebug.closed_loop_pid.d_term"));
            expectedRuleMzRef.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.control.MzRefDebug.closed_loop_yaw_ref"));
            expectedRuleMzRef.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.control.MzRefDebug.open_loop_pid.p_term"));
            expectedRuleMzRef.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.control.MzRefDebug.open_loop_pid.i_term"));
            expectedRuleMzRef.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.control.MzRefDebug.open_loop_pid.d_term"));

            yield return new object[] { new Dictionary<string, CompoundType> { { fullNameMzRef, compoundTypeMzRef }, { fullNamePid, compoundTypePid } }, subjectIdMzRef, fullNameMzRef, expectedRuleMzRef, wrongSubjectId, wrongName };
        }

        [DataTestMethod]
        [DynamicData(nameof(GetDsdlAndResultForService), DynamicDataSourceType.Method)]
        public void GenerateSingleServiceSerializationRule(
            Dictionary<string, CompoundType> dsdlDict,
            uint expectedSubjectId,
            string expectedServiceName,
            List<UavcanChannel> expectedRequestSerializationRule,
            List<UavcanChannel> expectedResponseSerializationRule,
            uint wrongSubjectId,
            string wrongServiceName)
        {
            var dsdlParser = new Moq.Mock<IDsdlParser>();
            dsdlParser.Setup(d => d.ParsedDsdlDict).Returns(dsdlDict);
            var rulesGenerator = new UavcanSerializationRulesGenerator(dsdlParser.Object);

            try
            {
                rulesGenerator.Init();
            }
            catch (UavcanException e)
            {
                Assert.Fail("Expected no exception, but got: " + e.Message);
            }

            Assert.IsTrue(rulesGenerator.TryGetSerializationRuleForService(expectedSubjectId, out var idRules));
            Assert.IsTrue(rulesGenerator.TryGetSerializationRuleForService(expectedServiceName, out var nameRules));

            Assert.IsFalse(rulesGenerator.TryGetSerializationRuleForService(wrongSubjectId, out var _));
            Assert.IsFalse(rulesGenerator.TryGetSerializationRuleForService(wrongServiceName, out var _));

            CollectionAssert.AreEqual(expectedRequestSerializationRule, idRules.RequestFields);
            CollectionAssert.AreEqual(expectedResponseSerializationRule, idRules.ResponseFields);
            CollectionAssert.AreEqual(expectedRequestSerializationRule, nameRules.RequestFields);
            CollectionAssert.AreEqual(expectedResponseSerializationRule, nameRules.ResponseFields);
        }

        [TestMethod]
        [DynamicData(nameof(GetDsdlExceptions), DynamicDataSourceType.Method)]
        public void ThrowsUavcanExceptionWhenParserFails(DsdlException exception)
        {
            var stubParser = new Moq.Mock<IDsdlParser>();
            stubParser.Setup(_ => _.ParseAllDirectories()).Throws(exception);

            var rulesGenerator = new UavcanSerializationRulesGenerator(stubParser.Object);

            Assert.ThrowsException<UavcanException>(() => rulesGenerator.Init());

        }

        public static IEnumerable<object[]> GetDsdlExceptions()
        {
            yield return new object[] { new DsdlException("Error parsing dsdl") };
            yield return new object[] { new DsdlException("Error parsing dsdl", "filename") };
            yield return new object[] { new DsdlException("Error parsing dsdl", "filename", sourceLine: 1) };

        }

        public static IEnumerable<object[]> GetDsdlAndResultForService()
        {
            string fullName = "TestDsdl.dashboard.RTDS";
            string wrongName = "TestDsdl.incorrect.name";
            uint subjectId = 35;
            uint wrongSubjectId = 42;
            CompoundType compoundType = new CompoundType(fullName,
                MessageType.SERVICE,
                subjectId,
                new System.Tuple<int, int>(1, 0));

            compoundType.RequestFields.Add(new Field(new PrimitiveType(BaseType.UNSIGNED_INT, 8, CastMode.SATURATED), "command"));
            compoundType.ResponseFields.Add(new Field(new PrimitiveType(BaseType.UNSIGNED_INT, 8, CastMode.SATURATED), "success"));

            List<UavcanChannel> expectedRuleRequest = new List<UavcanChannel>() {
                new UavcanChannel(BaseType.UNSIGNED_INT, 8, "TestDsdl.dashboard.RTDS.command")
            };

            List<UavcanChannel> expectedRuleResponse = new List<UavcanChannel>() {
                new UavcanChannel(BaseType.UNSIGNED_INT, 8, "TestDsdl.dashboard.RTDS.success")
            };

            yield return new object[] { new Dictionary<string, CompoundType> { { fullName, compoundType } }, subjectId, fullName, expectedRuleRequest, expectedRuleResponse, wrongSubjectId, wrongName };
        }

        [TestMethod]
        [DynamicData(nameof(GetMessageSubjectIds), DynamicDataSourceType.Method)]
        public void GetMessageNameFromSubjectIdTest(Dictionary<string, CompoundType> dsdlDict, uint subjectId, string expectedName)
        {
            var dsdlParser = new Moq.Mock<IDsdlParser>();
            dsdlParser.Setup(d => d.ParsedDsdlDict).Returns(dsdlDict);
            var rulesGenerator = new UavcanSerializationRulesGenerator(dsdlParser.Object);

            try
            {
                rulesGenerator.Init();
            }
            catch (UavcanException e)
            {
                Assert.Fail("Expected no exception, but got: " + e.Message);
            }

            Assert.AreEqual(expectedName, rulesGenerator.GetMessageNameFromSubjectId(subjectId));
        }
        public static IEnumerable<object[]> GetMessageSubjectIds()
        {
            string fullName = "dashboard.RTDS";
            uint subjectId = 35;
            CompoundType compoundType = new CompoundType(fullName,
                MessageType.SERVICE,
                subjectId,
                new System.Tuple<int, int>(1, 0));

            string fullNameMessage = "PitotTube";
            uint subjectIdMessage = 413;
            CompoundType compoundTypeMessage = new CompoundType(fullNameMessage,
                MessageType.MESSAGE,
                subjectIdMessage,
                new System.Tuple<int, int>(1, 0));

            yield return new object[] { new Dictionary<string, CompoundType> { { fullName, compoundType }, { fullNameMessage, compoundTypeMessage } }, subjectIdMessage, fullNameMessage };
        }

        [TestMethod]
        [DynamicData(nameof(GetServiceSubjectIds), DynamicDataSourceType.Method)]
        public void GetServiceNameFromSubjectIdTest(Dictionary<string, CompoundType> dsdlDict, uint subjectId, string expectedName)
        {
            var dsdlParser = new Moq.Mock<IDsdlParser>();
            dsdlParser.Setup(d => d.ParsedDsdlDict).Returns(dsdlDict);
            var rulesGenerator = new UavcanSerializationRulesGenerator(dsdlParser.Object);

            try
            {
                rulesGenerator.Init();
            }
            catch (UavcanException e)
            {
                Assert.Fail("Expected no exception, but got: " + e.Message);
            }

            Assert.AreEqual(expectedName, rulesGenerator.GetServiceNameFromSubjectId(subjectId));
        }
        public static IEnumerable<object[]> GetServiceSubjectIds()
        {
            string fullName = "dashboard.RTDS";
            uint subjectId = 35;
            CompoundType compoundType = new CompoundType(fullName,
                MessageType.SERVICE,
                subjectId,
                new System.Tuple<int, int>(1, 0));

            string fullNameMessage = "PitotTube";
            uint subjectIdMessage = 413;
            CompoundType compoundTypeMessage = new CompoundType(fullNameMessage,
                MessageType.MESSAGE,
                subjectIdMessage,
                new System.Tuple<int, int>(1, 0));

            yield return new object[] { new Dictionary<string, CompoundType> { { fullName, compoundType }, { fullNameMessage, compoundTypeMessage } }, subjectId, fullName };
        }

        [TestMethod]
        [DynamicData(nameof(GetInvalidMessageSubjectIds), DynamicDataSourceType.Method)]
        public void FailToGetMessageNameFromSubjectIdTest(Dictionary<string, CompoundType> dsdlDict, uint subjectId)
        {
            var dsdlParser = new Moq.Mock<IDsdlParser>();
            dsdlParser.Setup(d => d.ParsedDsdlDict).Returns(dsdlDict);
            var rulesGenerator = new UavcanSerializationRulesGenerator(dsdlParser.Object);

            try
            {
                rulesGenerator.Init();
            }
            catch (UavcanException e)
            {
                Assert.Fail("Expected no exception, but got: " + e.Message);
            }

            Assert.AreEqual(string.Empty, rulesGenerator.GetMessageNameFromSubjectId(subjectId));
        }
        public static IEnumerable<object[]> GetInvalidMessageSubjectIds()
        {
            string fullName = "dashboard.RTDS";
            uint subjectId = 35;
            CompoundType compoundType = new CompoundType(fullName,
                MessageType.SERVICE,
                subjectId,
                new System.Tuple<int, int>(1, 0));

            string fullNameMessage = "PitotTube";
            uint subjectIdMessage = 413;
            CompoundType compoundTypeMessage = new CompoundType(fullNameMessage,
                MessageType.MESSAGE,
                subjectIdMessage,
                new System.Tuple<int, int>(1, 0));

            yield return new object[] { new Dictionary<string, CompoundType> { { fullName, compoundType }, { fullNameMessage, compoundTypeMessage } }, subjectIdMessage + 1 };
        }

        [TestMethod]
        [DynamicData(nameof(GetInvalidServiceSubjectIds), DynamicDataSourceType.Method)]
        public void FailToServiceNameFromSubjectIdTest(Dictionary<string, CompoundType> dsdlDict, uint subjectId)
        {
            var dsdlParser = new Moq.Mock<IDsdlParser>();
            dsdlParser.Setup(d => d.ParsedDsdlDict).Returns(dsdlDict);
            var rulesGenerator = new UavcanSerializationRulesGenerator(dsdlParser.Object);

            try
            {
                rulesGenerator.Init();
            }
            catch (UavcanException e)
            {
                Assert.Fail("Expected no exception, but got: " + e.Message);
            }

            Assert.AreEqual(string.Empty, rulesGenerator.GetServiceNameFromSubjectId(subjectId));
        }
        public static IEnumerable<object[]> GetInvalidServiceSubjectIds()
        {
            string fullName = "dashboard.RTDS";
            uint subjectId = 35;
            CompoundType compoundType = new CompoundType(fullName,
                MessageType.SERVICE,
                subjectId,
                new System.Tuple<int, int>(1, 0));

            string fullNameMessage = "PitotTube";
            uint subjectIdMessage = 413;
            CompoundType compoundTypeMessage = new CompoundType(fullNameMessage,
                MessageType.MESSAGE,
                subjectIdMessage,
                new System.Tuple<int, int>(1, 0));

            yield return new object[] { new Dictionary<string, CompoundType> { { fullName, compoundType }, { fullNameMessage, compoundTypeMessage } }, subjectId + 1 };
        }

    }
}
