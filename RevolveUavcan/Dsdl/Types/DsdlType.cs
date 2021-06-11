namespace RevolveUavcan.Dsdl.Types
{
    public enum Category
    {
        PRIMITIVE,
        ARRAY,
        COMPOUND,
        VOID
    }

    public enum CastMode
    {
        SATURATED,
        TRUNCATED
    }

    /// <summary>
    /// The base class for UAVCAN types
    /// Fields:
    ///     fullName    Full type name string
    ///     category    Any type category
    /// </summary>
    public abstract class DsdlType
    {
        public string fullName;


        public Category Category { get; set; }

        protected DsdlType(string fullName, Category category)
        {
            this.fullName = fullName;
            this.Category = category;
        }

        public abstract override string ToString();
        public abstract int GetMaxBitLength();
        public abstract int GetMinBitLength();
    }
}
