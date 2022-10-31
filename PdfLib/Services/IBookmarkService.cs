using WF.PdfLib.Services.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WF.PdfLib.Services
{
    /// <summary>
    /// Service for manipulating the bookmarks of a document.
    /// </summary>
    public interface IBookmarkService
    {
        /// <summary>
        /// Get <see cref="ILeveledBookmark"/>s from a document.
        /// </summary>
        /// <param name="sourcePdf">Document to get the bookmarks from.</param>
        /// <returns>A list of bookmarks found.</returns>
        public Task<IList<ILeveledBookmark>> FindBookmarks(string sourcePdf);

        /// <summary>
        /// Insert <see cref="ILeveledBookmark"/>s into a document.
        /// </summary>
        /// <param name="bookmarks">Bookmarks to add.</param>
        /// <param name="documentPath">The pdf to add the bookmarks to.</param>
        public void AddBookmarks(IList<ILeveledBookmark> bookmarks, string documentPath);
    }
}
