using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using RevolveUavcan.Dsdl.Types;
using RevolveUavcan.Uavcan;
using System.IO;
using RevolveUavcan.Dsdl.Fields;
using RevolveUavcan.Dsdl;
using RevolveUavcan.Dsdl.Interfaces;

namespace RevolveUavcanTest.Uavcan
{
    [TestClass]
    public class UavcanSerializationRulesGeneratorTests
    {
        [DataTestMethod]
        [DynamicData(nameof(GetDsdlAndResult), DynamicDataSourceType.Method)]
        [DeploymentItem("60.cinco.1.0.uavcan", "TestFiles/TestDsdl/common")]
        public void GenerateSingleMessageSerializationRule(Dictionary<string, CompoundType> dsdlDict, uint expectedSubjectId, string expectedMessageName, List<UavcanChannel> expectedSerializationRule)
        {
            var dsdlParser = new Moq.Mock<IDsdlParser>();
            dsdlParser.Setup(d => d.ParsedDsdlDict).Returns(dsdlDict);
            var rulesGenerator = new UavcanSerializationRulesGenerator(dsdlParser.Object);
            Assert.IsTrue(rulesGenerator.Init());

            Assert.IsTrue(rulesGenerator.TryGetSerializationRuleForMessage(expectedSubjectId, out var idRules));
            Assert.IsTrue(rulesGenerator.TryGetSerializationRuleForMessage(expectedMessageName, out var nameRules));

            //CollectionAssert.AreEqual(expectedSerializationRule, idRules);

            Assert.AreEqual(expectedSerializationRule.Count, idRules.Count);
            Assert.AreEqual(expectedSerializationRule.Count, nameRules.Count);

            for (int i = 0; i < expectedSerializationRule.Count; i++)
            {
                Assert.AreEqual(expectedSerializationRule[i].FieldName, idRules[i].FieldName);
                Assert.AreEqual(expectedSerializationRule[i].FieldName, nameRules[i].FieldName);

                Assert.AreEqual(expectedSerializationRule[i].Size, idRules[i].Size);
                Assert.AreEqual(expectedSerializationRule[i].Size, nameRules[i].Size);

                Assert.AreEqual(expectedSerializationRule[i].ArraySize, idRules[i].ArraySize);
                Assert.AreEqual(expectedSerializationRule[i].ArraySize, nameRules[i].ArraySize);

                Assert.AreEqual(expectedSerializationRule[i].Basetype, idRules[i].Basetype);
                Assert.AreEqual(expectedSerializationRule[i].Basetype, nameRules[i].Basetype);

                Assert.AreEqual(expectedSerializationRule[i].IsArray, idRules[i].IsArray);
                Assert.AreEqual(expectedSerializationRule[i].IsArray, nameRules[i].IsArray);

                Assert.AreEqual(expectedSerializationRule[i].IsDynamic, idRules[i].IsDynamic);
                Assert.AreEqual(expectedSerializationRule[i].IsDynamic, nameRules[i].IsDynamic);
            }
        }

        public static IEnumerable<object[]> GetDsdlAndResult()
        {
            // Compound type that is not a message
            string fullNamePid = "TestDsdl.control.PIDControl";
            uint subjectIdPid = 0;
            var compoundTypePid = new CompoundType(fullNamePid,
                MessageType.MESSAGE,
                @"TestFiles/TestDsdl/control/PIDControl.1.0.uavcan",
                subjectIdPid,
                new System.Tuple<int, int>(1, 0),
                File.ReadAllText(@"TestFiles/TestDsdl/control/PIDControl.1.0.uavcan"));

            compoundTypePid.requestFields.Add(new Field(new PrimitiveType(BaseType.FLOAT, 32, CastMode.SATURATED), "p_term"));
            compoundTypePid.requestFields.Add(new Field(new PrimitiveType(BaseType.FLOAT, 32, CastMode.SATURATED), "i_term"));
            compoundTypePid.requestFields.Add(new Field(new PrimitiveType(BaseType.FLOAT, 32, CastMode.SATURATED), "d_term"));


            string fullNameCinco = "TestDsdl.common.cinco";
            uint subjectIdCinco = 60;
            CompoundType compoundTypeCinco = new CompoundType(fullNameCinco,
                MessageType.MESSAGE,
                @"TestFiles/TestDsdl/common/60.cinco.1.0.uavcan",
                subjectIdCinco,
                new System.Tuple<int, int>(1, 0),
                File.ReadAllText(@"TestFiles/TestDsdl/common/60.cinco.1.0.uavcan"));
            compoundTypeCinco.requestFields.Add(new Field(new PrimitiveType(BaseType.SIGNED_INT, 3, CastMode.SATURATED), "a"));
            compoundTypeCinco.requestFields.Add(new Field(new PrimitiveType(BaseType.UNSIGNED_INT, 3, CastMode.SATURATED), "b"));
            compoundTypeCinco.requestFields.Add(new Field(new PrimitiveType(BaseType.FLOAT, 32, CastMode.SATURATED), "c"));
            compoundTypeCinco.requestFields.Add(new Field(new PrimitiveType(BaseType.FLOAT, 64, CastMode.SATURATED), "d"));
            compoundTypeCinco.requestFields.Add(new Field(new VoidType(16), ""));
            compoundTypeCinco.requestFields.Add(new Field(new PrimitiveType(BaseType.BOOLEAN, 1, CastMode.SATURATED), "f"));
            compoundTypeCinco.requestFields.Add(new Field(new ArrayType(new PrimitiveType(BaseType.UNSIGNED_INT, 8, CastMode.SATURATED), ArrayMode.STATIC, 3), "g"));
            compoundTypeCinco.requestFields.Add(new Field(new ArrayType(compoundTypePid, ArrayMode.STATIC, 2), "h"));

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

            yield return new object[] { new Dictionary<string, CompoundType> { { fullNameCinco, compoundTypeCinco } }, subjectIdCinco, fullNameCinco, expectedRuleCinco };

            // A compound type which has a compound type as field
            string fullNameMzRef = "TestDsdl.control.MzRefDebug";
            uint subjectIdMzRef = 136;
            var compoundTypeMzRef = new CompoundType(fullNameMzRef,
                MessageType.MESSAGE,
                @"TestFiles/TestDsdl/control/136.MzRefDebug.1.0.uavcan",
                subjectIdMzRef,
                new System.Tuple<int, int>(1, 0),
                File.ReadAllText(@"TestFiles/TestDsdl/control/136.MzRefDebug.1.0.uavcan"));

            compoundTypeMzRef.requestFields.Add(new Field(compoundTypePid, "closed_loop_pid"));
            compoundTypeMzRef.requestFields.Add(new Field(new PrimitiveType(BaseType.FLOAT, 32, CastMode.SATURATED), "closed_loop_yaw_ref"));
            compoundTypeMzRef.requestFields.Add(new Field(compoundTypePid, "open_loop_pid"));


            var expectedRuleMzRef = new List<UavcanChannel>();
            expectedRuleMzRef.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.control.MzRefDebug.closed_loop_pid.p_term"));
            expectedRuleMzRef.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.control.MzRefDebug.closed_loop_pid.i_term"));
            expectedRuleMzRef.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.control.MzRefDebug.closed_loop_pid.d_term"));
            expectedRuleMzRef.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.control.MzRefDebug.closed_loop_yaw_ref"));
            expectedRuleMzRef.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.control.MzRefDebug.open_loop_pid.p_term"));
            expectedRuleMzRef.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.control.MzRefDebug.open_loop_pid.i_term"));
            expectedRuleMzRef.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.control.MzRefDebug.open_loop_pid.d_term"));

            yield return new object[] { new Dictionary<string, CompoundType> { { fullNameMzRef, compoundTypeMzRef }, { fullNamePid, compoundTypePid } }, subjectIdMzRef, fullNameMzRef, expectedRuleMzRef };
        }

        [DataTestMethod]
        [DynamicData(nameof(GetDsdlAndResultForService), DynamicDataSourceType.Method)]
        public void GenerateSingleServiceSerializationRule(Dictionary<string, CompoundType> dsdlDict, uint expectedSubjectId, string expectedServiceName, List<UavcanChannel> expectedRequestSerializationRule, List<UavcanChannel> expectedResponseSerializationRule)
        {
            var dsdlParser = new Moq.Mock<IDsdlParser>();
            dsdlParser.Setup(d => d.ParsedDsdlDict).Returns(dsdlDict);
            var rulesGenerator = new UavcanSerializationRulesGenerator(dsdlParser.Object);
            Assert.IsTrue(rulesGenerator.Init());

            Assert.IsTrue(rulesGenerator.TryGetSerializationRuleForService(expectedSubjectId, out var idRules));
            Assert.IsTrue(rulesGenerator.TryGetSerializationRuleForService(expectedServiceName, out var nameRules));

            AssertEqualSerializationRules(idRules.RequestFields, expectedRequestSerializationRule);
            AssertEqualSerializationRules(idRules.ResponseFields, expectedResponseSerializationRule);

            AssertEqualSerializationRules(nameRules.RequestFields, expectedRequestSerializationRule);
            AssertEqualSerializationRules(nameRules.ResponseFields, expectedResponseSerializationRule);
        }

        public static IEnumerable<object[]> GetDsdlAndResultForService()
        {
            string fullName = "TestDsdl.dashboard.RTDS";
            uint subjectId = 35;
            CompoundType compoundType = new CompoundType(fullName,
                MessageType.SERVICE,
                @"TestFiles/TestDsdl/dashboard/35.RTDS.1.0.uavcan",
                subjectId,
                new System.Tuple<int, int>(1, 0),
                File.ReadAllText(@"TestFiles/TestDsdl/dashboard/35.RTDS.1.0.uavcan"));

            compoundType.requestFields.Add(new Field(new PrimitiveType(BaseType.UNSIGNED_INT, 8, CastMode.SATURATED), "command"));
            compoundType.responseFields.Add(new Field(new PrimitiveType(BaseType.UNSIGNED_INT, 8, CastMode.SATURATED), "success"));

            List<UavcanChannel> expectedRuleRequest = new List<UavcanChannel>() {
                new UavcanChannel(BaseType.UNSIGNED_INT, 8, "TestDsdl.dashboard.RTDS.command")
            };

            List<UavcanChannel> expectedRuleResponse = new List<UavcanChannel>() {
                new UavcanChannel(BaseType.UNSIGNED_INT, 8, "TestDsdl.dashboard.RTDS.success")
            };

            yield return new object[] { new Dictionary<string, CompoundType> { { fullName, compoundType } }, subjectId, fullName, expectedRuleRequest, expectedRuleResponse };
        }

        private void AssertEqualSerializationRules(List<UavcanChannel> expected, List<UavcanChannel> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i].FieldName, actual[i].FieldName);

                Assert.AreEqual(expected[i].Size, actual[i].Size);

                Assert.AreEqual(expected[i].ArraySize, actual[i].ArraySize);

                Assert.AreEqual(expected[i].Basetype, actual[i].Basetype);

                Assert.AreEqual(expected[i].IsArray, actual[i].IsArray);

                Assert.AreEqual(expected[i].IsDynamic, actual[i].IsDynamic);
            }
        }
    }
}
