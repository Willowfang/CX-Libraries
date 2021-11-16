using CX.PdfLib.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.PdfLib.Services
{
    public interface IBookmarker
    {
        /// <summary>
        /// Get leveled bookmarks from a document
        /// </summary>
        /// <param name="sourcePdf">Document to get the bookmarks from</param>
        /// <returns>A list of bookmarks</returns>
        public IList<ILeveledBookmark> FindBookmarks(string sourcePdf);
        /// <summary>
        /// Add leveled bookmarks for a document
        /// </summary>
        /// <param name="bookmarks">Bookmarks to add</param>
        /// <param name="documentPath">The pdf to add the bookmarks to</param>
        public void AddBookmarks(IList<ILeveledBookmark> bookmarks, string documentPath);
    }
}
