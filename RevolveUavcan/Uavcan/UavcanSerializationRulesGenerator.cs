using RevolveUavcan.Dsdl.Fields;
using RevolveUavcan.Dsdl.Types;
using RevolveUavcan.Dsdl;
using System;
using System.Collections.Generic;
using System.Linq;
using RevolveUavcan.Uavcan.Interfaces;
using RevolveUavcan.Dsdl.Interfaces;

namespace RevolveUavcan.Uavcan
{
    public class UavcanSerializationRulesGenerator : IUavcanSerializationGenerator
    {
        public Dictionary<Tuple<uint, string>, List<UavcanChannel>> MessageSerializationRules { get; private set; }
        public Dictionary<Tuple<uint, string>, UavcanService> ServiceSerializationRules { get; private set; }
        public IDsdlParser DsdlParser { get; }
        public UavcanSerializationRulesGenerator(IDsdlParser dsdlParser)
        {
            DsdlParser = dsdlParser;

            MessageSerializationRules = new Dictionary<Tuple<uint, string>, List<UavcanChannel>>();
            ServiceSerializationRules = new Dictionary<Tuple<uint, string>, UavcanService>();
        }

        /// <inheritdoc/>
        public void Init()
        {
            try
            {
                DsdlParser.ParseAllDirectories();
                GenerateSerializationRulesForAllDsdl();
            }
            catch (DsdlException e)
            {
                throw new UavcanException(e.ToString());
            }
        }

        public bool TryGetSerializationRuleForMessage(uint subjectId, out List<UavcanChannel> uavcanChannels)
        {
            var key = MessageSerializationRules.Keys.FirstOrDefault(x => x.Item1 == subjectId);
            if (key.Item1 == subjectId)
            {
                uavcanChannels = MessageSerializationRules[key];
                return true;
            }
            uavcanChannels = null;
            return false;
        }

        public bool TryGetSerializationRuleForMessage(string messageName, out List<UavcanChannel> uavcanChannels)
        {
            var key = MessageSerializationRules.Keys.FirstOrDefault(x => x.Item2 == messageName);
            if (key.Item2 == messageName)
            {
                uavcanChannels = MessageSerializationRules[key];
                return true;
            }
            uavcanChannels = null;
            return false;
        }

        public bool TryGetSerializationRuleForService(uint subjectId, out UavcanService service)
        {
            var key = ServiceSerializationRules.Keys.FirstOrDefault(x => x.Item1 == subjectId);
            if (key.Item1 == subjectId)
            {
                service = ServiceSerializationRules[key];
                return true;
            }
            service = null;
            return false;
        }

        public bool TryGetSerializationRuleForService(string serviceName, out UavcanService service)
        {
            var key = ServiceSerializationRules.Keys.FirstOrDefault(x => x.Item2 == serviceName);
            if (key.Item2 == serviceName)
            {
                service = ServiceSerializationRules[key];
                return true;
            }
            service = null;
            return false;
        }

        /// <summary>
        /// Uses the parsedDsdl data to create a dictionary with a file structure, and list consisting of
        /// basetype (bool, uint, int or float), their size and channel name (eg. value, x, roll, acceleration, etc).
        /// </summary>
        /// <returns></returns>
        private void GenerateSerializationRulesForAllDsdl()
        {
            foreach (var key in DsdlParser.ParsedDsdlDict.Keys.Where(key => DsdlParser.ParsedDsdlDict[key].SubjectId != 0))
            {
                var compoundType = DsdlParser.ParsedDsdlDict[key];

                var combinedKey = new Tuple<uint, string>(compoundType.SubjectId, key);
                if (compoundType.MessageType == MessageType.MESSAGE)
                {
                    MessageSerializationRules.Add(combinedKey, GenerateSerializationRulesForType(compoundType, false, key));
                }
                else
                {
                    var reqFields = GenerateSerializationRulesForType(compoundType, false, key);
                    var resFields = GenerateSerializationRulesForType(compoundType, true, key);
                    ServiceSerializationRules.Add(combinedKey,
                        new UavcanService(reqFields, resFields, compoundType.SubjectId,
                            compoundType.FullName));
                }
            }
        }

        /// <summary>
        /// Parses the file structure of the parsed dsdl-files (only requests) and retrieves the basetype, size and channel name.
        /// As some of the structs are compound types, recursion is used to fetch deep-layered structs.
        /// </summary>
        /// <param name="fieldList"></param>
        /// <param name="parentName"></param>
        /// <returns></returns>
        private List<UavcanChannel> GenerateSerializationRulesForType(CompoundType type, bool responseNotRequest = false, string parentName = "")
        {
            var fieldList = responseNotRequest ? type.ResponseFields : type.RequestFields;

            var outList = new List<UavcanChannel>();
            foreach (Field field in fieldList)
            {
                switch (field.type.Category)
                {
                    case Category.COMPOUND:
                        {
                            var fieldName = (!string.IsNullOrEmpty(parentName)) ? (parentName + "." + field.name) : (field.name);
                            outList.AddRange(GenerateSerializationRulesForType(field.type as CompoundType, false, fieldName));
                            break;
                        }
                    case Category.ARRAY:
                        {
                            if (field.type is ArrayType arrayType)
                            {
                                if (arrayType.dataType is PrimitiveType)
                                {
                                    outList.AddRange(GenerateSerializationRuleForPrimitiveArray(arrayType, field.name, parentName));
                                }
                                else if (arrayType.dataType is CompoundType)
                                {
                                    outList.AddRange(GenerateSerializationRuleForCompoundArray(arrayType, field.name, parentName));
                                }
                            }
                            break;
                        }
                    case Category.VOID:
                        {
                            outList.Add(new UavcanChannel(BaseType.VOID, field.type.GetMaxBitLength(), ""));
                            break;
                        }
                    case Category.PRIMITIVE:
                        {
                            outList.Add(GenerateSerializationRuleForPrimitiveType(field, parentName));
                            break;
                        }
                }
            }

            return outList;
        }


        private UavcanChannel GenerateSerializationRuleForPrimitiveType(Field field, string parentName)
        {
            var fieldName = parentName != "" ? parentName + "." + field.name : field.name;

            if (field.type is PrimitiveType primitiveType)
            {
                return new UavcanChannel(primitiveType.BaseType, primitiveType.GetMaxBitLength(), fieldName);
            }
            return null;
        }

        private List<UavcanChannel> GenerateSerializationRuleForPrimitiveArray(ArrayType arrayType, string fieldName, string parentName)
        {
            var serializationRule = new List<UavcanChannel>();

            if (arrayType != null)
            {
                var primitiveType = arrayType.dataType as PrimitiveType;

                if (primitiveType != null)
                {
                    for (int i = 0; i < arrayType.maxSize; i++)
                    {

                        serializationRule.Add(new UavcanChannel(primitiveType.BaseType,
                            primitiveType.GetMaxBitLength(),
                            (!string.IsNullOrEmpty(parentName) ? (parentName + "." + fieldName) : (fieldName)) +
                            "_" + i));
                    }
                    return serializationRule;
                }
            }
            throw new UavcanException("Invalid array definition, cannot generate serialization rule");
        }

        private List<UavcanChannel> GenerateSerializationRuleForCompoundArray(ArrayType arrayType, string fieldName, string parentName)
        {
            var serializationRule = new List<UavcanChannel>();

            if (arrayType.dataType is CompoundType compoundType)
            {
                for (int i = 0; i < arrayType.maxSize; i++)
                {
                    var compoundRule = GenerateSerializationRulesForType(compoundType, false, $"{parentName}.{fieldName}_{i}");
                    serializationRule.AddRange(compoundRule.Select(channel => new UavcanChannel(channel.Basetype,
                        channel.Size, channel.FieldName)));
                }
                return serializationRule;
            }
            throw new UavcanException("Invalid array definition, cannot generate serialization rule");
        }
    }
}
