namespace RevolveUavcan.Dsdl.Types
{
    public class VoidType : DsdlType
    {
        private readonly int bitLength;

        public VoidType(int bitLength) : base(GetNormalizedDefinition(bitLength), Category.VOID) => this.bitLength = bitLength;

        public static string GetNormalizedDefinition(int bitLength) => $"void{bitLength}";

        public override string ToString() => GetNormalizedDefinition(bitLength);

        public override int GetMaxBitLength() => bitLength;

        public override int GetMinBitLength() => bitLength;
    }
}
