using RevolveUavcan.Dsdl.Fields;
using RevolveUavcan.Dsdl.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace RevolveUavcan.Dsdl
{
    public class DsdlRuleGenerator
    {
        public static string REQUEST_PREFIX = "request_";
        public static string RESPONSE_PREFIX = "response_";

        private Dictionary<string, CompoundType> _parsedDsdl;

        private Dictionary<Tuple<uint, string>, List<UavcanChannel>> _flattenedDsdlMessages;
        private Dictionary<Tuple<uint, string>, UavcanService> _flattenedServices;
        private DsdlParser _parser { get; }

        private readonly ILogger _logger;

        public DsdlRuleGenerator(string dsdlPath, ILogger logger)
        {
            _parser = new DsdlParser(dsdlPath);
            _logger = logger;

            _flattenedDsdlMessages = new Dictionary<Tuple<uint, string>, List<UavcanChannel>>();
            _flattenedServices = new Dictionary<Tuple<uint, string>, UavcanService>();
        }


        public bool InitDsdlRules()
        {
            try
            {
                _parsedDsdl = _parser.ParseAllDirectories();
                FlattenDictionary();
            }
            catch (DsdlException)
            {
                return false;
            }

            return true;
        }

        public bool GetSerializationRuleForMessage(uint subjectId, out List<UavcanChannel> uavcanChannels)
        {
            var key = _flattenedDsdlMessages.Keys.FirstOrDefault(x => x.Item1 == subjectId);
            if (key.Item1 == subjectId)
            {
                uavcanChannels = _flattenedDsdlMessages[key];
                return true;
            }
            uavcanChannels = new List<UavcanChannel>();
            return false;
        }

        public bool GetSerializationRuleForMessage(string messageName, out List<UavcanChannel> uavcanChannels)
        {
            var key = _flattenedDsdlMessages.Keys.FirstOrDefault(x => x.Item2 == messageName);
            if (key.Item2 == messageName)
            {
                uavcanChannels = _flattenedDsdlMessages[key];
                return true;
            }
            uavcanChannels = new List<UavcanChannel>();
            return false;
        }

        public bool GetSerializationRuleForService(uint subjectId, out UavcanService service)
        {
            var key = _flattenedServices.Keys.FirstOrDefault(x => x.Item1 == subjectId);
            if (key.Item1 == subjectId)
            {
                service = _flattenedServices[key];
                return true;
            }
            service = new UavcanService(new List<UavcanChannel>(), new List<UavcanChannel>(), 0, "");
            return false;
        }

        public bool GetSerializationRuleForService(string serviceName, out UavcanService service)
        {
            var key = _flattenedServices.Keys.FirstOrDefault(x => x.Item2 == serviceName);
            if (key.Item2 == serviceName)
            {
                service = _flattenedServices[key];
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
        private void FlattenDictionary()
        {
            foreach (var key in _parsedDsdl.Keys.Where(key => _parsedDsdl[key].defaultDataTypeID != 0))
            {
                var compoundType = _parsedDsdl[key];

                var combinedKey = new Tuple<uint, string>(compoundType.defaultDataTypeID, key);
                if (compoundType.messageType == MessageType.MESSAGE)
                {
                    _flattenedDsdlMessages.Add(combinedKey, FlattenFieldList(compoundType.requestFields, key));
                }
                else
                {
                    var reqFields = FlattenFieldList(compoundType.requestFields, key);
                    var resFields = FlattenFieldList(compoundType.responseFields, key);
                    _flattenedServices.Add(combinedKey,
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
        private List<UavcanChannel> FlattenFieldList(List<Field> fieldList, string parentName = "")
        {
            var outList = new List<UavcanChannel>();
            foreach (Field field in fieldList)
            {
                if (field.type.Category == Category.COMPOUND)
                {
                    var fieldName = (parentName != "") ? (parentName + "." + field.name) : (field.name);
                    var list = FlattenFieldList((field.type as CompoundType).requestFields, fieldName);
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
                            var flattened = FlattenFieldList(compoundType?.responseFields, compoundType.FullName);
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
