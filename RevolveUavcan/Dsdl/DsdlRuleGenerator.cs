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

        public Dictionary<string, CompoundType> ParsedDsdl { get; private set; }

        public Dictionary<string, List<UavcanChannel>> FlattenedDsdlMessages { get; private set; }
        public Dictionary<string, UavcanService> FlattenedServices { get; private set; }

        public Dictionary<uint, string> MessageDataIdMap { get; private set; }
        public Dictionary<string, uint> MessageDataIdMapReversed { get; set; }
        public Dictionary<uint, string> ServiceDataIdMap { get; private set; }
        public Dictionary<string, uint> ServiceDataIdMapReversed { get; set; }

        public Dictionary<string, int> NodeNameToNodeId { get; set; }

        public DsdlParser Parser { get; }

        private readonly ILogger _logger;

        public DsdlRuleGenerator(string dsdlPath, ILogger logger)
        {
            Parser = new DsdlParser(dsdlPath);
            _logger = logger;

            FlattenedDsdlMessages = new Dictionary<string, List<UavcanChannel>>();
            FlattenedServices = new Dictionary<string, UavcanService>();
            MessageDataIdMap = new Dictionary<uint, string>();
            MessageDataIdMapReversed = new Dictionary<string, uint>();
            ServiceDataIdMapReversed = new Dictionary<string, uint>();
            ServiceDataIdMap = new Dictionary<uint, string>();
        }


        public bool InitDsdlRules()
        {
            try
            {
                ParsedDsdl = Parser.ParseAllDirectories();
                GenerateDataIdMap();
                FlattenDictionary();
                GenerateListOfNodes();
            }
            catch (DsdlException)
            {
                // Toast.Error($"DSDL ERROR: {e}", true);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generates a list of nodes to send Uavcan service from
        /// </summary>
        /// <returns></returns>
        private void GenerateListOfNodes()
        {
            // Gets all nodes defined in a DSDL file
            var hasNodeIds = ParsedDsdl.TryGetValue("common.Systems",
                out var nodeIds);

            if (hasNodeIds)
            {
                NodeNameToNodeId = new Dictionary<string, int>();

                foreach (var constant in nodeIds.requestConstants)
                {
                    try
                    {
                        NodeNameToNodeId.Add(constant.name, int.Parse(constant.StringValue));
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
            }
            else
            {
                throw new DsdlException("No node IDs found.");
            }
        }

        public bool ReInitDsdlRules()
        {
            FlattenedDsdlMessages = new Dictionary<string, List<UavcanChannel>>();
            FlattenedServices = new Dictionary<string, UavcanService>();
            MessageDataIdMap = new Dictionary<uint, string>();
            MessageDataIdMapReversed = new Dictionary<string, uint>();
            ServiceDataIdMapReversed = new Dictionary<string, uint>();
            ServiceDataIdMap = new Dictionary<uint, string>();
            return InitDsdlRules();
        }

        /// <summary>
        /// Maps all the DSDL-files with their respective DataTypeID,
        /// one for messages and one for services.
        /// </summary>
        public void GenerateDataIdMap()
        {
            foreach (var key in ParsedDsdl.Keys)
            {
                // defaultDataTypeID is set to -1 if the UAVCAN file associated with it does not
                // contain a DataTypeID. Then it is just a datatype, and not a message/service
                var canId = ParsedDsdl[key].defaultDataTypeID;
                if (canId == 0)
                {
                    continue;
                }

                if (ParsedDsdl[key].messageType == MessageType.MESSAGE)
                {
                    if (MessageDataIdMap.ContainsKey(canId))
                    {
                        throw new DsdlException("Two or more message use the same messageID: " + canId.ToString());
                    }

                    MessageDataIdMap.Add(canId, key);
                    MessageDataIdMapReversed.Add(key, canId);
                }
                else
                {
                    if (ServiceDataIdMap.ContainsKey(canId))
                    {
                        throw new DsdlException("Two or more services use the same messageID: " + canId.ToString());
                    }

                    ServiceDataIdMap.Add(canId, key);
                    ServiceDataIdMapReversed.Add(key, canId);
                }
            }
        }

        /// <summary>
        /// Generates data channels for all fields in the DSDL messages with a messageTypeId. These will be used by the
        /// AnalyzeDataModel.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, UavcanChannel> GenerateDsdlDatachannelDict()
        {
            var outDict = new Dictionary<string, UavcanChannel>();

            foreach (var keyValuePair in FlattenedDsdlMessages)
            {
                var baseName = keyValuePair.Key;
                var value = keyValuePair.Value;
                foreach (var uavcanChannel in value.Where(uavcanChannel => uavcanChannel.Basetype != BaseType.VOID))
                {
                    if (MessageDataIdMapReversed.TryGetValue(baseName, out var canId))
                    {
                        if (uavcanChannel.IsDynamic)
                        {
                            for (int i = 0; i < uavcanChannel.ArraySize; i++)
                            {
                                var FullName = uavcanChannel.FieldName + "_" + i;
                                try
                                {
                                    outDict.Add(FullName, uavcanChannel);
                                }
                                catch (Exception)
                                {
                                    throw new DsdlException("Two or more messages use the same name: " + FullName);
                                }
                            }
                        }
                        else
                        {
                            var FullName = uavcanChannel.FieldName;
                            try
                            {
                                outDict.Add(FullName, uavcanChannel);
                            }
                            catch (Exception)
                            {
                                throw new DsdlException("Two or more messages use the same name: " + FullName);
                            }
                        }
                    }
                }
            }

            foreach (var keyValuePair in FlattenedServices)
            {
                var baseName = keyValuePair.Key;
                var value = keyValuePair.Value;
                foreach (var uavcanChannel in value.ResponseFields.Where(uavcanChannel =>
                    uavcanChannel.Basetype != BaseType.VOID))
                {
                    if (ServiceDataIdMapReversed.TryGetValue(baseName, out var canId))
                    {
                        if (uavcanChannel.IsDynamic)
                        {
                            for (int i = 0; i < uavcanChannel.ArraySize; i++)
                            {
                                var FullName = RESPONSE_PREFIX + uavcanChannel.FieldName + "_" + i;
                                try
                                {
                                    outDict.Add(FullName, uavcanChannel);
                                }
                                catch (Exception)
                                {
                                    throw new DsdlException("Two or more messages use the same name: " + FullName);
                                }
                            }
                        }
                        else
                        {
                            var FullName = RESPONSE_PREFIX + uavcanChannel.FieldName;
                            try
                            {
                                outDict.Add(FullName, uavcanChannel);
                            }
                            catch (Exception)
                            {
                                throw new DsdlException("Two or more messages use the same name: " + FullName);
                            }
                        }
                    }
                }

                var uavcanChannelsWithoutBasetypeVoid
                    = value.RequestFields.Where(uavcanChannel => uavcanChannel.Basetype != BaseType.VOID);

                foreach (var uavcanChannel in uavcanChannelsWithoutBasetypeVoid)
                {
                    if (ServiceDataIdMapReversed.TryGetValue(baseName, out var canId))
                    {
                        if (uavcanChannel.IsDynamic)
                        {
                            for (int i = 0; i < uavcanChannel.ArraySize; i++)
                            {
                                var FullName = REQUEST_PREFIX + uavcanChannel.FieldName + "_" + i;
                                try
                                {
                                    outDict.Add(FullName, uavcanChannel);
                                }
                                catch (Exception)
                                {
                                    throw new DsdlException("Two or more messages use the same name: " + FullName);
                                }
                            }
                        }
                        else
                        {
                            var FullName = REQUEST_PREFIX + uavcanChannel.FieldName;
                            try
                            {
                                outDict.Add(FullName, uavcanChannel);
                            }
                            catch (Exception)
                            {
                                throw new DsdlException("Two or more messages use the same name: " + FullName);
                            }
                        }
                    }
                }
            }

            return outDict;
        }

        /// <summary>
        /// Uses the parsedDsdl data to create a dictionary with a file structure, and list consisting of
        /// basetype (bool, uint, int or float), their size and channel name (eg. value, x, roll, acceleration, etc).
        /// </summary>
        /// <returns></returns>
        public void FlattenDictionary()
        {
            foreach (var key in ParsedDsdl.Keys.Where(key => ParsedDsdl[key].defaultDataTypeID != 0))
            {
                if (ParsedDsdl[key].messageType == MessageType.MESSAGE)
                {
                    FlattenedDsdlMessages.Add(key, FlattenFieldList(ParsedDsdl[key].requestFields, key));
                }
                else
                {
                    var reqFields = FlattenFieldList(ParsedDsdl[key].requestFields, key);
                    var resFields = FlattenFieldList(ParsedDsdl[key].responseFields, key);
                    FlattenedServices.Add(key,
                        new UavcanService(reqFields, resFields, ParsedDsdl[key].defaultDataTypeID,
                            ParsedDsdl[key].FullName));
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
                    foreach (var tuple in list)
                    {
                        outList.Add(tuple);
                    }
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
    }
}
