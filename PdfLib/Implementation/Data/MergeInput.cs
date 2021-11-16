using CX.PdfLib.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.PdfLib.Implementation.Data
{
    /// <summary>
    /// Default implementation for <see cref="IMergeInput"/>
    /// </summary>
    public class MergeInput : IMergeInput
    {
        public string FilePath { get; }
        public string Title { get; }
        public int Level { get; }

        public MergeInput(string filePath, string title, int level)
        {
            FilePath = filePath;
            Title = title;
            Level = level;
        }
    }
}
