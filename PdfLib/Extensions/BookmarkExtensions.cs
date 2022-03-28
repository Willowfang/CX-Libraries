using CX.PdfLib.Common;
using CX.PdfLib.Services.Data;
using System.Collections.Generic;

namespace CX.PdfLib.Extensions
{
    public static class BookmarkExtensions
    {
        /// <summary>
        /// Adjust the level of all bookmarks in an <see cref="IList{ILeveledBookmark}"/>.
        /// </summary>
        /// <param name="originals"></param>
        /// <param name="adjustment">Amount to adjust (positive or negative)</param>
        /// <remarks>Bookmark level cannot be lower than 1. Bookmarks will be adjusted
        /// to level 1, if adjustment would bring it lower. This can be used to level all
        /// bookmarks to level 1.</remarks>
        /// <returns></returns>
        public static IList<ILeveledBookmark> AdjustLevels(this IEnumerable<ILeveledBookmark> originals, 
            int adjustment)
        {
            List<ILeveledBookmark> adjusted = new List<ILeveledBookmark>();
            foreach (ILeveledBookmark bookmark in originals)
            {
                int adjLevel = bookmark.Level + adjustment > 0 ? bookmark.Level + adjustment : 1;
                adjusted.Add(new LeveledBookmark(adjLevel, bookmark.Title,
                    bookmark.Pages));
            }

            return adjusted;
        }

        public static bool IsChild(this ILeveledBookmark current, IEnumerable<ILeveledBookmark> bookmarks)
        {
            foreach (ILeveledBookmark bookmark in bookmarks)
            {
                if (current.Level > bookmark.Level 
                    && current.StartPage >= bookmark.StartPage 
                    && current.EndPage <= bookmark.EndPage)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
