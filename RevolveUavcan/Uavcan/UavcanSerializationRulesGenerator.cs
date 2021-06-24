using RevolveUavcan.Dsdl.Fields;
using RevolveUavcan.Dsdl.Types;
using RevolveUavcan.Dsdl;
using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace RevolveUavcan.Uavcan
{
    public class UavcanSerializationRulesGenerator
    {
        public static string REQUEST_PREFIX = "request_";
        public static string RESPONSE_PREFIX = "response_";

        public Dictionary<Tuple<uint, string>, List<UavcanChannel>> MessageSerializationRules { get; private set; }
        public Dictionary<Tuple<uint, string>, UavcanService> ServiceSerializationRules { get; private set; }

        private Dictionary<string, CompoundType> _parsedDsdl;
        private readonly ILogger _logger;

        public UavcanSerializationRulesGenerator(Dictionary<string, CompoundType> parsedDsdl)
        {
            _parsedDsdl = parsedDsdl;

            MessageSerializationRules = new Dictionary<Tuple<uint, string>, List<UavcanChannel>>();
            ServiceSerializationRules = new Dictionary<Tuple<uint, string>, UavcanService>();
        }

        public bool Init()
        {
            try
            {
                GenerateSerializationRulesForAllDsdl();
                return true;
            }
            catch (DsdlException)
            {
                return false;
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
            uavcanChannels = new List<UavcanChannel>();
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
            uavcanChannels = new List<UavcanChannel>();
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
            service = new UavcanService(new List<UavcanChannel>(), new List<UavcanChannel>(), 0, "");
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
            service = new UavcanService(new List<UavcanChannel>(), new List<UavcanChannel>(), 0, "");
            return false;
        }

        /// <summary>
        /// Uses the parsedDsdl data to create a dictionary with a file structure, and list consisting of
        /// basetype (bool, uint, int or float), their size and channel name (eg. value, x, roll, acceleration, etc).
        /// </summary>
        /// <returns></returns>
        private void GenerateSerializationRulesForAllDsdl()
        {
            foreach (var key in _parsedDsdl.Keys.Where(key => _parsedDsdl[key].defaultDataTypeID != 0))
            {
                var compoundType = _parsedDsdl[key];

                var combinedKey = new Tuple<uint, string>(compoundType.defaultDataTypeID, key);
                if (compoundType.messageType == MessageType.MESSAGE)
                {
                    MessageSerializationRules.Add(combinedKey, GenerateSerializationRulesForType(compoundType, false, key));
                }
                else
                {
                    var reqFields = GenerateSerializationRulesForType(compoundType, false, key);
                    var resFields = GenerateSerializationRulesForType(compoundType, true, key);
                    ServiceSerializationRules.Add(combinedKey,
                        new UavcanService(reqFields, resFields, compoundType.defaultDataTypeID,
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
            var fieldList = responseNotRequest ? type.responseFields : type.requestFields;

            var outList = new List<UavcanChannel>();
            foreach (Field field in fieldList)
            {
                if (field.type.Category == Category.COMPOUND)
                {
                    var fieldName = (parentName != "") ? (parentName + "." + field.name) : (field.name);
                    // Only the parent can be a service, so we are always interested in request fields here
                    var list = GenerateSerializationRulesForType(field.type as CompoundType, false, fieldName);
                    outList.AddRange(list);
                }

                else if (field.type.Category == Category.ARRAY)
                {
                    var arrayType = field.type as ArrayType;
                    if (arrayType != null && arrayType.dataType.Category == Category.PRIMITIVE)
                    {
                        var primitiveType = arrayType.dataType as PrimitiveType;

                        if (arrayType.mode == ArrayMode.DYNAMIC)
                        {
                            if (primitiveType != null)
                            {
                                outList.Add(new UavcanChannel(primitiveType.baseType,
                                    primitiveType.GetMaxBitLength(),
                                    ((parentName != "")
                                        ? (parentName + "." + field.name)
                                        : (field.name)),
                                    true, arrayType.maxSize, true));
                            }
                        }
                        else
                        {
                            for (int i = 0; i < arrayType.maxSize; i++)
                            {
                                if (primitiveType != null)
                                {
                                    outList.Add(new UavcanChannel(primitiveType.baseType,
                                        primitiveType.GetMaxBitLength(),
                                        (parentName != "" ? (parentName + "." + field.name) : (field.name)) +
                                        "_" + i));
                                }
                            }
                        }
                    }
                    else
                    {
                        if (arrayType.dataType is CompoundType compoundType)
                        {
                            var flattened = GenerateSerializationRulesForType(compoundType, false, compoundType.FullName);
                            for (int i = 0; i < arrayType.maxSize; i++)
                            {
                                outList.AddRange(flattened.Select(channel => new UavcanChannel(channel.Basetype,
                                    channel.Size, compoundType.FullName + channel.FieldName + "." + i)));
                            }
                        }
                    }
                }
                else if (field.type.Category == Category.VOID)
                {
                    outList.Add(new UavcanChannel(BaseType.VOID, field.type.GetMaxBitLength(), ""));
                }
                else
                {
                    var fieldName = parentName != "" ? parentName + "." + field.name : field.name;

                    if (field.type is PrimitiveType primitiveType)
                    {
                        outList.Add(new UavcanChannel(primitiveType.baseType, primitiveType.GetMaxBitLength(),
                            fieldName));
                    }
                }
            }

            return outList;
        }


        /// <summary>
        /// Generates a list of nodes to send Uavcan service from
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, uint> GenerateListOfNodes(string nodeDefFile)
        {
            // Gets all nodes defined in a DSDL file
            var hasNodeIds = _parsedDsdl.TryGetValue(nodeDefFile,
                out var nodeIds);

            if (hasNodeIds)
            {
                var nodeNameToNodeId = new Dictionary<string, uint>();

                foreach (var constant in nodeIds.requestConstants)
                {
                    try
                    {
                        nodeNameToNodeId.Add(constant.name, uint.Parse(constant.StringValue));
                    }
                    catch (ArgumentNullException)
                    {
                        _logger.Warn("Constant's string value cannot be null.");
                    }
                    catch (ArgumentException)
                    {
                        _logger.Warn("Not a NumberStyles value.");
                    }
                    catch (FormatException)
                    {
                        _logger.Warn("Constant's string value is not format compliant.");
                    }
                    catch (OverflowException)
                    {
                        _logger
                            .Warn("Constant's string value is either less thna MinValue or greater than MaxValue.");
                    }
                }
                return nodeNameToNodeId;
            }
            else
            {
                throw new DsdlException("No node IDs found.");
            }
        }
    }
}