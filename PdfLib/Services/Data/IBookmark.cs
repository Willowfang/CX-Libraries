using System.Collections.Generic;

namespace CX.PdfLib.Services.Data
{
    /// <summary>
    /// Interface for a generic bookmark.
    /// </summary>
    public interface IBookmark
    {
        /// <summary>
        /// Name of the bookmark.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Pages in the bookmark page range.
        /// </summary>
        public IList<int> Pages { get; }

        /// <summary>
        /// The first page in the bookmark page range.
        /// </summary>
        public int StartPage { get; }

        /// <summary>
        /// Get last page in the bookmark page range.
        /// </summary>
        public int EndPage { get; }
    }
}
