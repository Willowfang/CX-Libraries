using CX.PdfLib.Services.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CX.PdfLib.Services
{
    public interface IBookmarkService
    {
        /// <summary>
        /// Get leveled bookmarks from a document
        /// </summary>
        /// <param name="sourcePdf">Document to get the bookmarks from</param>
        /// <returns>A list of bookmarks</returns>
        public Task<IList<ILeveledBookmark>> FindBookmarks(string sourcePdf);
        /// <summary>
        /// Insert leveled bookmarks into a document
        /// </summary>
        /// <param name="bookmarks">Bookmarks to add</param>
        /// <param name="documentPath">The pdf to add the bookmarks to</param>
        public void AddBookmarks(IList<ILeveledBookmark> bookmarks, string documentPath);
    }
}
