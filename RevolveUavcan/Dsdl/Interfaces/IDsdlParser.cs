using RevolveUavcan.Dsdl.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace RevolveUavcan.Dsdl.Interfaces
{
    public interface IDsdlParser
    {
        Dictionary<string, CompoundType> ParsedDsdlDict { get; }


        /// <summary>
        /// Where the DSDL files to parse are located, defaults to the standard
        /// path where the files from GitHub are located
        /// </summary>
        string DsdlPath { get; set; }


        /// <summary>
        /// Parses all files and subfiles from the folder specified by the <see cref="DsdlPath"/>
        /// </summary>
        /// <exception cref="DsdlException">At least one of the dsdl files are invalid, or the folder does not exist </exception>
        void ParseAllDirectories();


        /// <summary>
        /// Parse the entire source file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="sourceText"></param>
        /// <returns>The <see cref="CompoundType"/> derrived from the file </returns>
        /// <exception cref="DsdlException">The dsdl file is invalid or does not exist </exception>
        CompoundType ParseSource(string filename, string sourceText);
    }
}
