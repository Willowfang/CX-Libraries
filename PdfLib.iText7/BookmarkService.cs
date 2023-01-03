using WF.LoggingLib;
using WF.PdfLib.Common;
using WF.PdfLib.Services;
using WF.PdfLib.Services.Data;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Navigation;
using WF.LoggingLib.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace WF.PdfLib.iText7
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
            return await Task.Run(() => utilities.FindLeveledBookmarks(new PdfDocument(new PdfReader(sourcePdf)))) ??
                new List<ILeveledBookmark>();
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ILeveledBookmark DocumentAsBookmark(string path)
        {
            PdfDocument source = new PdfDocument(new PdfReader(path));

            int pageCount = source.GetNumberOfPages();
            string title = Path.GetFileNameWithoutExtension(path);

            return new LeveledBookmark(1, title, 1, pageCount);
        }
    }
}
