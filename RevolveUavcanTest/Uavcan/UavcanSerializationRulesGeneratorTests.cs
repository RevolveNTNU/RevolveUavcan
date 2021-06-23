using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using RevolveUavcan.Dsdl.Types;
using RevolveUavcan.Uavcan;
using System.IO;
using RevolveUavcan.Dsdl.Fields;

namespace RevolveUavcanTest.Uavcan
{
    [TestClass]
    public class UavcanSerializationRulesGeneratorTests
    {
        [DataTestMethod]
        [DynamicData(nameof(GetUavcanChannelsAndValues), DynamicDataSourceType.Method)]
        [DeploymentItem("60.cinco.1.0.uavcan", "TestFiles/TestDsdl/common")]
        public void GenerateSingleMessageSerializationRule(Dictionary<string, CompoundType> dsdlDict, uint expectedSubjectId, string expectedMessageName, List<UavcanChannel> expectedSerializationRule)
        {
            var rulesGenerator = new UavcanSerializationRulesGenerator(dsdlDict);
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

        public static IEnumerable<object[]> GetUavcanChannelsAndValues()
        {
            CompoundType compoundType = new CompoundType("TestDsdl.common.cinco",
                MessageType.MESSAGE,
                @"TestFiles/TestDsdl/common/60.cinco.1.0.uavcan",
                60,
                new System.Tuple<int, int>(1, 0),
                File.ReadAllText(@"TestFiles/TestDsdl/common/60.cinco.1.0.uavcan"));
            compoundType.requestFields.Add(new Field(new PrimitiveType(BaseType.SIGNED_INT, 3, CastMode.SATURATED), "a"));
            compoundType.requestFields.Add(new Field(new PrimitiveType(BaseType.UNSIGNED_INT, 3, CastMode.SATURATED), "b"));
            compoundType.requestFields.Add(new Field(new PrimitiveType(BaseType.FLOAT, 32, CastMode.SATURATED), "c"));
            compoundType.requestFields.Add(new Field(new PrimitiveType(BaseType.FLOAT, 64, CastMode.SATURATED), "d"));
            compoundType.requestFields.Add(new Field(new VoidType(16), ""));
            compoundType.requestFields.Add(new Field(new PrimitiveType(BaseType.BOOLEAN, 1, CastMode.SATURATED), "f"));

            List<UavcanChannel> expectedRule = new List<UavcanChannel>();
            expectedRule.Add(new UavcanChannel(BaseType.SIGNED_INT, 3, "TestDsdl.common.cinco.a"));
            expectedRule.Add(new UavcanChannel(BaseType.UNSIGNED_INT, 3, "TestDsdl.common.cinco.b"));
            expectedRule.Add(new UavcanChannel(BaseType.FLOAT, 32, "TestDsdl.common.cinco.c"));
            expectedRule.Add(new UavcanChannel(BaseType.FLOAT, 64, "TestDsdl.common.cinco.d"));
            expectedRule.Add(new UavcanChannel(BaseType.VOID, 16, ""));
            expectedRule.Add(new UavcanChannel(BaseType.BOOLEAN, 1, "TestDsdl.common.cinco.f"));


            yield return new object[] { new Dictionary<string, CompoundType> { { compoundType.FullName, compoundType } }, (uint)60, "TestDsdl.common.cinco", expectedRule };
        }
    }
}
