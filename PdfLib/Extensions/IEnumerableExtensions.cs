using WF.PdfLib.Services.Data;
using System.Collections.Generic;
using System.Linq;

namespace WF.PdfLib.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Find all children of a bookmarks from this <see cref="IEnumerable{ILeveledBookmark}"/>.
        /// </summary>
        /// <param name="original">The <see cref="IEnumerable{ILeveledBookmark}"/> to search in.</param>
        /// <param name="parent">The bookmarks whose children are to be found.</param>
        /// <returns>All children found.</returns>
        public static IList<ILeveledBookmark> Children(this IEnumerable<ILeveledBookmark> original,
            ILeveledBookmark parent)
        {
            return original.Where(x => x.StartPage >= parent.StartPage &&
                x.EndPage <= parent.StartPage &&
                x.Level > parent.Level).ToList();
        }
    }
}
