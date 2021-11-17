using CX.PdfLib.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CX.PdfLib.Extensions;

namespace CX.PdfLib.Implementation.Data
{
    /// <summary>
    /// Default implementation of <see cref="IExtractRange"/>
    /// </summary>
    public class ExtractRange : IExtractRange
    {
        public string Name { get; }
        public IList<int> Pages { get; }

        public ExtractRange(string name, IList<int> pages)
        {
            Name = name;
            Pages = pages;
        }
        public ExtractRange(string name, int startPage, int endPage)
        {
            Name = name;
            Pages = RangeList.Create(startPage, endPage);
        }
    }
}
