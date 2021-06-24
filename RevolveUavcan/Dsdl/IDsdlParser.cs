using RevolveUavcan.Dsdl.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace RevolveUavcan.Dsdl
{
    public interface IDsdlParser
    {
        public Dictionary<string, CompoundType> ParsedDsdlDict { get; }
        public string DsdlPath { get; set; }

        public Dictionary<string, CompoundType> ParseAllDirectories();

        public CompoundType ParseSource(string filename, string sourceText);
    }
}
