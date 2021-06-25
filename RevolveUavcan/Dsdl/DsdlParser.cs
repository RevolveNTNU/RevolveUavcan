using RevolveUavcan.Dsdl.Fields;
using RevolveUavcan.Dsdl.Interfaces;
using RevolveUavcan.Dsdl.Types;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Attribute = RevolveUavcan.Dsdl.Fields.Attribute;

namespace RevolveUavcan.Dsdl
{
    public class DsdlParser : IDsdlParser
    {
        public Dictionary<string, CompoundType> ParsedDsdlDict { get; private set; } = new Dictionary<string, CompoundType>();

        /// <summary>
        /// Where the DSDL files to parse are located, defaults to the standard
        /// path where the files from GitHub are located
        /// </summary>
        public string DsdlPath { get; set; }

        public DsdlParser(string dsdlPath)
        {
            DsdlPath = dsdlPath;
        }

        public void ParseAllDirectories()
        {
            // Reset the dictionary of parsed dsdl rules
            ParsedDsdlDict = new Dictionary<string, CompoundType>();

            // Return immediately if the DSDL directory doesn't exist
            if (!Directory.Exists(DsdlPath))
            {
                throw new DsdlException($"Dsdl Path: {DsdlPath} could not be found!");
            }

            // Parse each .uavcan file in the directory, including subdirectories
            var dirs = Directory.GetDirectories(DsdlPath, "*", SearchOption.AllDirectories);
            foreach (var file in dirs
                .SelectMany(Directory.GetFiles)
                .Where(fileName => fileName.Contains(".uavcan")))
            {
                var (fullName, _, _) = FullTypenameVersionAndDtidFromFilename(file);
                if (!ParsedDsdlDict.ContainsKey(fullName))
                {
                    var dsdlSource = ReadDsdlFile(file);
                    CompoundType type = ParseSource(file, dsdlSource);
                    ParsedDsdlDict.Add(type.FullName, type);
                }
            }
        }


        /// <summary>
        /// Reads the entirety of the file given
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string ReadDsdlFile(string filename)
        {
            string sourceText;
            try
            {
                filename = Path.GetFullPath(filename);
                sourceText = File.ReadAllText(filename);
            }
            catch (Exception e)
            {
                throw new DsdlException($"Failed to parse file: {filename}, error: {e}");
            }

            return sourceText;
        }


        /// <summary>
        /// Creates a python-style set for listing attribute names
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class Set<T> : SortedDictionary<T, bool>
        {
            public void Add(T item) => Add(item, true);
        }


        /// <summary>
        /// TODO: Comment this
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private List<List<string>> Tokenize(string text)
        {
            List<List<string>> lines = new List<List<string>>();
            Regex r = new Regex(@"#.*");
            foreach (string line in text.Split(Environment.NewLine.ToCharArray()))
            {
                var tempLine = r.Replace(line, "").Trim();
                if (tempLine != "")
                {
                    var list = tempLine.Split().ToList();
                    var storageList = new List<string>();
                    foreach (var token in list)
                    {
                        if (token != "")
                        {
                            storageList.Add(token);
                        }
                    }

                    lines.Add(storageList);
                }
            }

            return lines;
        }


        /// <summary>
        /// Parse the entire source file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="sourceText"></param>
        /// <returns></returns>
        public CompoundType ParseSource(string filename, string sourceText)
        {
            (var fullTypeName, Tuple<int, int> version, var defaultDtid) =
                FullTypenameVersionAndDtidFromFilename(filename);
            List<List<string>> dsdlLines = Tokenize(sourceText);

            var allAttributeNames = new Set<string>();
            var fields = new List<Field>();
            var constants = new List<Constant>();
            var respFields = new List<Field>();
            var respConstants = new List<Constant>();
            var union = false;
            var respUnion = false;
            var hasResponsePart = false;

            var messageSize = 0;
            var responseSize = 0;

            for (int i = 0; i < dsdlLines.Count; i++)
            {
                var line = dsdlLines[i];

                if (line[0] == "---" && line.Count == 1)
                {
                    if (hasResponsePart)
                    {
                        throw new DsdlException("A file can only have 1 responsepart", filename);
                    }

                    hasResponsePart = true;
                    allAttributeNames = new Set<string>();
                    continue;
                }

                if (line[0] == "@union" && line.Count == 1)
                {
                    if (hasResponsePart)
                    {
                        respUnion = true;
                    }
                    else
                    {
                        union = true;
                    }

                    continue;
                }

                if (line[0] == "@assert" || line[0] == "@sealed")
                {
                    continue;
                }

                Attribute attr = ParseLine(filename, line, i);

                if (attr.name == "")
                {
                    attr.name = $"void{i}";
                }


                if (attr.name != "" && allAttributeNames.ContainsKey(attr.name))
                {
                    throw new DsdlException($"Attributename {attr.name} is already registered.", filename,
                        i);
                }


                allAttributeNames.Add(attr.name);

                if (attr.isConstant)
                {
                    if (hasResponsePart)
                    {
                        respConstants.Add(attr as Constant);
                    }
                    else
                    {
                        constants.Add(attr as Constant);
                    }
                }
                else
                {
                    if (hasResponsePart)
                    {
                        // Adds the field to the Response Fields in this UAVCAN message
                        // Padding is added according to the serialisation rules in the
                        // UAVCAN specification, chapter 3.7:
                        // https://uavcan.org/specification/UAVCAN_Specification_v1.0-beta.pdf

                        var field = attr as Field;
                        if (field == null)
                        {
                            throw new DsdlException($"Error in parsing Response of {attr.name}.", filename, i);
                        }

                        if (TryAddPadding(field, responseSize, out var prePadding))
                        {
                            respFields.Add(prePadding);
                            responseSize += prePadding.type.GetMaxBitLength();
                        }

                        respFields.Add(field);

                        responseSize += field.type.GetMaxBitLength();

                        if (TryAddPadding(field, responseSize, out var postPadding))
                        {
                            respFields.Add(postPadding);
                            responseSize += postPadding.type.GetMaxBitLength();
                        }
                    }
                    else
                    {
                        // Adds the field to the Response Fields in this UAVCAN message
                        // Padding is added according to the serialisation rules in the
                        // UAVCAN specification, chapter 3.7:
                        // https://uavcan.org/specification/UAVCAN_Specification_v1.0-beta.pdf
                        var field = attr as Field;

                        if (field == null)
                        {
                            throw new DsdlException($"Error in parsing Message of {attr.name}.", filename, i);
                        }

                        if (TryAddPadding(field, messageSize, out var prePadding))
                        {
                            fields.Add(prePadding);
                            messageSize += prePadding.type.GetMaxBitLength();
                        }

                        fields.Add(field);

                        if (field != null)
                        {
                            messageSize += field.type.GetMaxBitLength();

                            if (TryAddPadding(field, messageSize, out var postPadding))
                            {
                                fields.Add(postPadding);
                                messageSize += postPadding.type.GetMaxBitLength();
                            }
                        }
                    }
                }
            }

            CompoundType t;

            if (hasResponsePart)
            {
                t = new CompoundType(fullTypeName, MessageType.SERVICE, defaultDtid, version)
                {
                    RequestFields = fields,
                    RequestConstants = constants,
                    ResponseFields = respFields,
                    ResponseConstants = respConstants,
                    RequestUnion = union,
                    ResponseUnion = respUnion
                };
            }
            else
            {
                t = new CompoundType(fullTypeName, MessageType.MESSAGE, defaultDtid, version)
                {
                    RequestFields = fields,
                    RequestConstants = constants,
                    RequestUnion = union
                };
            }

            return t;
        }

        /// <summary>
        /// Checks if padding has to be added using the serialisation rules from the
        /// UAVCAN specification, chapter 3.7.
        /// https://uavcan.org/specification/UAVCAN_Specification_v1.0-beta.pdf
        /// This is only the case if a field is a compound type. Then padding needs to be
        /// added so that the entire compound type is serialised in its own bytes. These
        /// bytes cannot contain data from other fields. The Revolve Analyze parser ensures
        /// this by added Void types to fill the byte before the compound type, and the last
        /// byte of the compound type.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="responseSize"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        private bool TryAddPadding(Field field, int responseSize, out Field padding)
        {
            if (field.type.Category == Category.COMPOUND &&
                responseSize % 8 != 0)
            {
                padding = new Field(ParseVoidType(8 - (responseSize % 8)), "");
                return true;
            }

            padding = null;
            return false;
        }

        private Attribute ParseLine(string filename, List<string> tokens, int lineNumber)
        {
            CastMode cm = CastMode.SATURATED;
            if (tokens[0] == "saturated" || tokens[0] == "truncated")
            {
                cm = tokens[0] == "saturated"
                    ? CastMode.SATURATED
                    : CastMode.TRUNCATED; // ASK LARS FOR PRETTIGRAPHICS.PNG
                tokens = tokens.Skip(1).ToList();
            }

            if (tokens.Count < 2 && !tokens[0].StartsWith("void"))
            {
                throw new DsdlException($"Syntaxerror in {filename}, have you forgotten to name the field?",
                    string.Join(" ", tokens));
            }

            string typename;
            string attrname;

            if (tokens.Count == 1)
            {
                typename = tokens[0];
                attrname = "";
                tokens = new List<string>();
            }
            else
            {
                typename = tokens[0];
                attrname = tokens[1];
                tokens = tokens.Skip(2).ToList();
            }

            var attrtype = ParseType(filename, typename, cm);

            if (tokens.Count > 0)
            {
                if (tokens.Count < 2 || tokens[0] != "=")
                {
                    throw new DsdlException($"Syntaxerror in line {lineNumber} : {string.Join(" ", tokens)}", filename);
                }

                var expression = string.Join(" ", tokens.Skip(1));
                return MakeConstant(attrtype, attrname, expression);
            }

            return new Field(attrtype, attrname);
        }


        private Constant MakeConstant(DsdlType attrType, string name, string expression)
        {
            if (attrType.Category != Category.PRIMITIVE)
            {
                throw new DsdlException($"Constant {name} has to be a primitive type");
            }

            PrimitiveType type = attrType as PrimitiveType;

            expression = string.Join("", expression.Split());
            ExpandoObject value = EvaluateExpression(expression, type.BaseType);
            return new Constant(type, name, value, expression);
        }


        #region TypeParsing

        /// <summary>
        /// Create a VoidType
        /// </summary>
        /// <param name="bitLength"></param>
        /// <returns>VoidType with length bitLength</returns>
        private VoidType ParseVoidType(int bitLength)
        {
            if (bitLength > 64 || bitLength <= 0)
            {
                throw new DsdlException("Bitsize out of range [1, 64]");
            }

            return new VoidType(bitLength);
        }

        /// <summary>
        /// Parse a PrimaryType
        /// </summary>
        /// <param name="baseName"></param>
        /// <param name="bitLength"></param>
        /// <param name="castMode"></param>
        /// <returns>Returns a PrimitiveType</returns>
        private PrimitiveType ParsePrimitiveType(string baseName, int bitLength, CastMode castMode)
        {
            switch (baseName)
            {
                case "bool":
                    return new PrimitiveType(BaseType.BOOLEAN, 1, castMode);
                case "int":
                    if (bitLength <= 64 && bitLength > 1)
                    {
                        return new PrimitiveType(BaseType.SIGNED_INT, bitLength, castMode);
                    }

                    break;
                case "uint":
                    if (bitLength <= 64 && bitLength > 1)
                    {
                        return new PrimitiveType(BaseType.UNSIGNED_INT, bitLength, castMode);
                    }

                    break;
                case "float":
                    if (bitLength == 16 || bitLength == 32 || bitLength == 64)
                    {
                        return new PrimitiveType(BaseType.FLOAT, bitLength, castMode);
                    }

                    break;
                default:
                    throw new DsdlException("Datatype has to be a primitive type: int, uint, bool or float");
            }

            return null;
        }

        /// <summary>
        /// Parse an ArrayType from the dsdl.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="baseName"></param>
        /// <param name="sizeStr"></param>
        /// <param name="castMode"></param>
        /// <returns></returns>
        private ArrayType ParseArrayType(string fileName, string baseName, string sizeStr, CastMode castMode)
        {
            var valueBaseType = ParseType(fileName, baseName, castMode);

            var maxSize = 0;
            var mode = ArrayMode.DYNAMIC;
            try
            {
                if (sizeStr.StartsWith("<="))
                {
                    maxSize = int.Parse(sizeStr.Substring(2));
                }
                else if (sizeStr.StartsWith("<"))
                {
                    maxSize = int.Parse(sizeStr.Substring(1)) - 1;
                }
                else
                {
                    maxSize = int.Parse(sizeStr);
                    mode = ArrayMode.STATIC;
                }
            }
            catch (Exception)
            {
                throw new DsdlException("Syntaxerror on arraydefinition, has to be [x], [<x] or ");
            }

            return new ArrayType(valueBaseType, mode, maxSize);
        }

        private CompoundType ParseCompoundType(string fileName, string rawTypeDef)
        {
            var defFilename = LocateCompoundTypeDefinition(fileName, rawTypeDef);
            var (fullName, _, _) = FullTypenameVersionAndDtidFromFilename(defFilename);
            CompoundType type;
            if (!ParsedDsdlDict.TryGetValue(fullName, out type))
            {
                var dsdlSource = ReadDsdlFile(defFilename);
                type = ParseSource(defFilename, dsdlSource);
                ParsedDsdlDict.Add(type.FullName, type);
            }

            if (type.MessageType == MessageType.MESSAGE)
            {
                return type;
            }

            throw new DsdlException("A service type cannot be merged into another compound type");
        }

        /// <summary>
        /// Parse a type from a DSDL line.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="rawTypeDef"></param>
        /// <param name="castMode"></param>
        /// <returns>Returns a DsdlType from the DSDL line</returns>
        private DsdlType ParseType(string fileName, string rawTypeDef, CastMode castMode)
        {
            var typeDef = rawTypeDef.Trim();
            var voidMatch = new Regex(@"void(\d{1,2})$").Match(typeDef);
            var arrayMatch = new Regex(@"(.+?)\[([^\]]*)\]$").Match(typeDef);
            var primitiveMatch = new Regex(@"([a-z]+)(\d{1,2})$|(bool)$").Match(typeDef);

            if (voidMatch.Length != 0)
            {
                var bits = voidMatch.Groups[1].ToString().Trim();
                return ParseVoidType(int.Parse(bits));
            }

            if (arrayMatch.Length != 0)
            {
                if (primitiveMatch.Length != 0)
                {
                    throw new DsdlException("Syntax error", fileName);
                }

                var valueType = arrayMatch.Groups[1].ToString().Trim();
                var size = arrayMatch.Groups[2].ToString();
                return ParseArrayType(fileName, valueType, size, castMode);
            }

            if (primitiveMatch.Length != 0)
            {
                if (primitiveMatch.Groups[0].ToString() == "bool")
                {
                    return ParsePrimitiveType("bool", 1, castMode);
                }

                var valueType = primitiveMatch.Groups[1].ToString().Trim();
                var size = primitiveMatch.Groups[2].ToString().Trim();
                return ParsePrimitiveType(valueType, int.Parse(size), castMode);
            }

            return ParseCompoundType(fileName, typeDef);
        }

        #endregion

        #region HelperFunctions

        private string LocateCompoundTypeDefinition(string refFilename, string rawTypeDef)
        {
            string fullTypeName;

            if (!rawTypeDef.Contains(".") || (int.TryParse(rawTypeDef.Split('.').ToArray().Last(), out var _) &&
                                              rawTypeDef.Split('.').Length == 3))
            {
                var currentNamespace = NamespaceFromFilename(refFilename);
                fullTypeName = currentNamespace + "." + rawTypeDef;
            }
            else
            {
                fullTypeName = rawTypeDef;
            }

            string nameSpace;
            var path = fullTypeName.Split('.').ToArray();
            // If the file has a version spesified, we dont need to also skip that in the namespace name
            if (int.TryParse(path.Last(), out var n))
            {
                nameSpace = string.Join(".", path.Take(path.Length - 3));
            }
            else
            {
                nameSpace = string.Join(".", path.Take(path.Length - 1));
            }

            var directory = LocateNamespaceDirectory(nameSpace, refFilename);

            DirectoryInfo d = new DirectoryInfo(directory);
            var files = d.GetFiles("*.uavcan");
            foreach (var filename in files)
            {
                var splittedName = filename.ToString().Split('.').ToList();
                string file = (splittedName.Count == 3) ? splittedName[1] + "." + splittedName[2] : filename.ToString();
                string shortFileName = Path.GetFileName(file);

                string rawFile = (rawTypeDef + ".uavcan");

                if (rawFile.Contains("." + shortFileName) || rawFile.Contains("/" + shortFileName) ||
                    rawFile == shortFileName)
                {
                    return filename.ToString();
                }
            }

            return "";
        }

        private string NamespaceFromFilename(string refFilename)
        {
            var filename = Path.GetFullPath(refFilename);

            var directory = Path.GetFullPath(DsdlPath);
            var rootNs = directory.Split(Path.DirectorySeparatorChar).Last();
            if (filename.StartsWith(directory))
            {
                var dirLen = directory.Length;
                var basenameLen = Path.GetFileName(filename).Length;
                var ns = filename.Substring(dirLen, filename.Length - dirLen - basenameLen);
                ns = (ns.Replace(Path.DirectorySeparatorChar, '.')).Trim('.');
                return ns;
            }


            throw new DsdlException("File was not found in the search directories");
        }

        private string LocateNamespaceDirectory(string nameSpace, string refFilename)
        {
            var nameSpaceFolder = nameSpace.Replace('.', Path.DirectorySeparatorChar);

            var rootdirectory = Path.GetFullPath(DsdlPath);
            if (DsdlPath == nameSpaceFolder)
            {
                return rootdirectory;
            }

            var fullPath = Path.Combine(rootdirectory, nameSpaceFolder);

            foreach (var dir in Directory.GetDirectories(DsdlPath, "*", SearchOption.AllDirectories))
            {
                var directory = Path.GetFullPath(dir);
                if (directory == fullPath)
                {
                    return directory;
                }
            }

            throw new DsdlException($"Unknown namespace ({nameSpace}) for " + refFilename);
        }

        private ExpandoObject EvaluateExpression(string expression, BaseType baseType)
        {
            dynamic returnValue = new ExpandoObject();

            int baseSize = 10;
            if (expression.Length > 2)
            {
                if (expression.Substring(0, 2) == "0x")
                {
                    baseSize = 16;
                }
                else if (expression.Substring(0, 2) == "0b")
                {
                    baseSize = 2;
                }
                else if (expression.Substring(0, 2) == "0o")
                {
                    baseSize = 8;
                }
            }
            if (baseSize != 10)
            {
                expression = expression.Substring(2);
            }

            switch (baseType)
            {
                case BaseType.FLOAT:
                    returnValue.value = float.Parse(expression.Replace(".", ","));
                    break;
                case BaseType.SIGNED_INT:
                    if (baseSize != 10)
                    {
                        returnValue.value = Convert.ToInt64(expression, baseSize);
                    }
                    else
                    {
                        returnValue.value = long.Parse(expression);
                    }

                    break;
                case BaseType.UNSIGNED_INT:
                    if (expression[0] == '\'')
                    {
                        var val = Regex.Unescape(expression.Trim('\''));

                        char character = val[0];
                        returnValue.value = character;
                        break;
                    }

                    if (baseSize != 10)
                    {
                        returnValue.value = Convert.ToUInt64(expression, baseSize);
                    }
                    else
                    {
                        returnValue.value = ulong.Parse(expression);
                    }

                    break;
                case BaseType.BOOLEAN:
                    returnValue.value = bool.Parse(expression);
                    break;
            }

            return returnValue;
        }

        private Tuple<string, Tuple<int, int>, uint> FullTypenameVersionAndDtidFromFilename(string filename)
        {
            string basename = Path.GetFileName(filename);

            var items = basename.Split('.');
            if (items.Length != 2 && items.Length != 3 && items.Length != 4 && items.Length != 5 ||
                items.Last() != "uavcan")
            {
                throw new DsdlException("Only .uavcan files can be parsed!");
            }

            uint defaultDataID = 0;

            string name;
            if (items.Length == 2 || items.Length == 4)
            {
                name = items[0];
            }
            else
            {
                var defaultDataIDString = items[0];
                name = items[1];
                try
                {
                    defaultDataID = uint.Parse(defaultDataIDString);
                }
                catch (Exception)
                {
                    throw new DsdlException("Wrong datatypeID format, has to be an integer");
                }
            }

            Tuple<int, int> version = new Tuple<int, int>(0, 0);
            if (items.Length == 2 || items.Length == 3)
            {
            }
            else
            {
                string minor, major;
                minor = items[items.Length - 2];
                major = items[items.Length - 3];
                try
                {
                    version = new Tuple<int, int>(int.Parse(major), int.Parse(minor));
                }
                catch (Exception)
                {
                    throw new DsdlException("Wrong version syntax. Has to be X.Y");
                }
            }

            string fullName = NamespaceFromFilename(filename) + "." + name;

            return new Tuple<string, Tuple<int, int>, uint>(fullName, version, defaultDataID);
        }

        #endregion
    }
}
