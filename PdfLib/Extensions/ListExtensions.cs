using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.PdfLib.Extensions
{
    public static class ListExtensions
    {
        public static List<int> Range(this List<int> list, int start, int count)
        {
            return Enumerable.Range(start, count).ToList();
        }
    }

    public static class RangeList
    {
        public static List<int> Create(int start, int count)
        {
            return Enumerable.Range(start, count).ToList();
        }
    }
}
