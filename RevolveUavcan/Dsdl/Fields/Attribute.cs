using RevolveUavcan.Dsdl.Types;

namespace RevolveUavcan.Dsdl.Fields
{
    /// <summary>
    /// Base class for attributes.
    /// </summary>
    public class Attribute
    {
        public DsdlType type;
        public string name;
        public readonly bool isConstant;

        protected Attribute(DsdlType type, string name, bool isConstant)
        {
            this.type = type;
            this.name = name;
            this.isConstant = isConstant;
        }
    }
}
