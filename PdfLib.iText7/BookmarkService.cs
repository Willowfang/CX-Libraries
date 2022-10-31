using CX.LoggingLib;
using CX.PdfLib.Common;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Navigation;
using LoggingLib.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CX.PdfLib.iText7
{
    /// <summary>
    /// Default implementation for <see cref="BookmarkService"/>.
    /// </summary>
    public class BookmarkService : LoggingEnabled<BookmarkService>, IBookmarkService
    {
        private Utilities utilities;

        /// <summary>
        /// Create a new instance of bookmark service.
        /// </summary>
        /// <param name="logbook">Logging service.</param>
        public BookmarkService(ILogbook logbook) : base(logbook)
        {
            utilities = new Utilities(logbook);
        }

        /// <summary>
        /// Get all bookmarks from a document.
        /// </summary>
        /// <param name="sourcePdf">Source document path.</param>
        /// <returns>A list of all found bookmarks.</returns>
        public async Task<IList<ILeveledBookmark>> FindBookmarks(string sourcePdf)
        {
            return await Task.Run(() => utilities.FindLeveledBookmarks(new PdfDocument(new PdfReader(sourcePdf))));
        }

        /// <summary>
        /// Add bookmarks to a document.
        /// </summary>
        /// <param name="bookmarks">Bookmarks to add.</param>
        /// <param name="documentPath">Destination document path.</param>
        public void AddBookmarks(IList<ILeveledBookmark> bookmarks, string documentPath)
        {
            PdfDocument product = new PdfDocument(new PdfWriter(documentPath));
            utilities.AddLeveledBookmarks(bookmarks, product);
            product.Close();
        }
    }
}
