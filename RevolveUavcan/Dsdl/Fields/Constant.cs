using RevolveUavcan.Dsdl.Types;
using System.Dynamic;

namespace RevolveUavcan.Dsdl.Fields
{
    public class Constant : Attribute
    {
        public readonly ExpandoObject value;

        private string _stringValue;

        public string StringValue
        {
            get => _stringValue;
            set => _stringValue = value;
        }

        private readonly string _initExpression;


        /// <summary>
        /// I am fully aware of the high quality
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="expandoObject"></param>
        /// <param name="initExpression"></param>
        public Constant(DsdlType type, string name, ExpandoObject expandoObject, string initExpression) : base(type,
            name, true)
        {
            value = expandoObject;
            _stringValue = ((dynamic)expandoObject).value.ToString();
            _initExpression = initExpression;
        }

        public override string ToString() => $"{type} {name} {_initExpression}";
    }
}
