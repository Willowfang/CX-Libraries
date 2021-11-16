using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CX.PdfLib.Extensions;

namespace CX.PdfLib.Implementation.Data
{
    /// <summary>
    /// Base class for bookmarks
    /// </summary>
    public abstract class Bookmark
    {
        /// <summary>
        /// The title of the bookmark
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// Number of the page where the bookmarks starts at
        /// </summary>
        public IList<int> Pages { get; }

        public Bookmark(string title, IList<int> pages)
        {
            Title = title;
            Pages = pages;
        }

        public Bookmark(string title, int startPage, int pageCount)
        {
            Title = title;
            Pages = new List<int>().Range(startPage, pageCount);
        }
    }
}
