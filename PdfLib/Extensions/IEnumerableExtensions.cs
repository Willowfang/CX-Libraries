using CX.PdfLib.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.PdfLib.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IList<ILeveledBookmark> Children(this IEnumerable<ILeveledBookmark> original,
            ILeveledBookmark parent)
        {
            return original.Where(x => x.StartPage >= parent.StartPage &&
                x.EndPage <= parent.StartPage &&
                x.Level > parent.Level).ToList();
        }
    }
}
