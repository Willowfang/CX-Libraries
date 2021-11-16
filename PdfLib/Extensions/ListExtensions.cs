using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.PdfLib.Extensions
{
    internal static class ListExtensions
    {
        internal static List<int> Range(this List<int> list, int start, int end)
        {
            return Enumerable.Range(start, end).ToList();
        }
    }
}
