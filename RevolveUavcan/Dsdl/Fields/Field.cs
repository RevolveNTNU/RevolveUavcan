using RevolveUavcan.Dsdl.Types;

namespace RevolveUavcan.Dsdl.Fields
{
    public class Field : Attribute
    {
        public Field(DsdlType type, string name) : base(type, name, false)
        {
        }

        public override string ToString()
        {
            if (type.Category == Category.VOID)
            {
                return type.ToString();
            }

            return $"{type.ToString()} {name}";
        }
    }
}
