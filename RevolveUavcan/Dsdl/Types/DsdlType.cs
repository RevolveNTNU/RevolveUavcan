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
        public string FullName { get; }


        public Category Category { get; }

        protected DsdlType(string fullName, Category category)
        {
            FullName = fullName;
            Category = category;
        }

        public abstract override string ToString();
        public abstract int GetMaxBitLength();
        public abstract int GetMinBitLength();
    }
}
