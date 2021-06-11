using System;
using System.Collections.Generic;

namespace RevolveUavcan.Dsdl.Types
{
    public enum BaseType
    {
        BOOLEAN,
        UNSIGNED_INT,
        SIGNED_INT,
        FLOAT,
        VOID
    }

    /// <summary>
    /// Type class for the primitive datatypes. These include: bool, int, uint, float
    /// </summary>
    public class PrimitiveType : DsdlType
    {
        public BaseType baseType;
        private readonly int bitLength;
        private readonly CastMode castMode;
        private readonly Tuple<double, double> valueRange;

        public static Dictionary<BaseType, string> BaseTypeNames = new Dictionary<BaseType, string> {
                        { BaseType.FLOAT, "float" },
                        { BaseType.SIGNED_INT, "int" },
                        { BaseType.UNSIGNED_INT, "uint" },
                        { BaseType.BOOLEAN, "bool" },
                        {BaseType.VOID, "void"}
        };

        public PrimitiveType(BaseType baseType, int bitLength, CastMode castMode) :
                        base(GetNormalizedDefinition(castMode, baseType), Category.PRIMITIVE)
        {
            this.baseType = baseType;
            this.bitLength = bitLength;
            this.castMode = castMode;
            valueRange = TypeLimits.getRange(baseType, bitLength);
        }

        public override string ToString() => GetNormalizedDefinition(castMode, baseType);

        public override int GetMaxBitLength() => bitLength;

        public override int GetMinBitLength() => bitLength;

        public static string GetNormalizedDefinition(CastMode castMode, BaseType baseType)
        {
            var castModeString = castMode == CastMode.SATURATED ? "saturated" : "truncated";
            var baseTypeName = BaseTypeNames[baseType];
            return castModeString + " " + baseTypeName;
        }
    }
}
