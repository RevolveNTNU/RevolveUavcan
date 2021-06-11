using System;

namespace RevolveUavcan.Dsdl
{
    public class DsdlException : Exception
    {
        public string filename;
        private readonly int sourceLine;

        public DsdlException(string message, string filename = "", int sourceLine = -1) : base(message)
        {
            this.filename = filename;
            this.sourceLine = sourceLine;
        }

        public override string ToString()
        {
            if (filename != "" && sourceLine != -1)
            {
                return $"{filename}:{sourceLine}: {Message}";
            }

            return filename != "" ? $"{filename}: {Message}" : Message;
        }
    }
}
