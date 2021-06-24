using RevolveUavcan.Dsdl.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace RevolveUavcan.Dsdl.Interfaces
{
    public interface IDsdlParser
    {
        Dictionary<string, CompoundType> ParsedDsdlDict { get; }
        string DsdlPath { get; set; }

        void ParseAllDirectories();
        CompoundType ParseSource(string filename, string sourceText);
    }
}
