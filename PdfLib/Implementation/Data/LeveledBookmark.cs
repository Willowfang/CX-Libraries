using CX.PdfLib.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.PdfLib.Implementation.Data
{
    /// <summary>
    /// A class for bookmarks categorised by their level
    /// </summary>
    public class LeveledBookmark : Bookmark, ILeveledBookmark
    {
        /// <summary>
        /// Level of the bookmark in the bookmark tree (1 leftmost)
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Create info on leveled bookmark
        /// </summary>
        /// <param name="level">Bookmark level in the bookmark tree</param>
        /// <param name="title">Title of the bookmark</param>
        /// <param name="pages">Pages contained in the bookmark</param>
        public LeveledBookmark(int level, string title, IList<int> pages)
            : base(title, pages) => Level = level;
        /// <summary>
        /// Create info on leveled bookmark
        /// </summary>
        /// <param name="level">Bookmark level in the bookmark tree</param>
        /// <param name="title">Title of the bookmark</param>
        /// <param name="startPage">Start page for bookmarks</param>
        /// <param name="pageCount">Number of pages in the range. Throws <see cref="ArgumentException"/>
        /// if <paramref name="pageCount"/> is negative.</param>
        /// <exception cref="ArgumentException">Thrown, if <paramref name="pageCount"/> is negative.</exception>
        public LeveledBookmark(int level, string title, int startPage, int pageCount)
            : base(title, startPage, pageCount) => Level = level;
    }
}
