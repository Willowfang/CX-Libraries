using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int StartPage => Pages[0];
        /// <summary>
        /// Get last page
        /// </summary>
        public int EndPage => Pages.Last();
    }
}
