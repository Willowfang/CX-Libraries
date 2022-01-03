using System.Collections.Generic;

namespace CX.PdfLib.Services.Data
{
    /// <summary>
    /// Generic bookmark service
    /// </summary>
    public interface IBookmark
    {
        /// <summary>
        /// Name of the bookmark
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// Pages contained in the bookmark
        /// </summary>
        public IList<int> Pages { get; }
        /// <summary>
        /// Get first page (i.e. destination)
        /// </summary>
        public int StartPage { get; }
        /// <summary>
        /// Get last page
        /// </summary>
        public int EndPage { get; }
    }
}
