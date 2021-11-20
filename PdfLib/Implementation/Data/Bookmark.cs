using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CX.PdfLib.Extensions;
using CX.PdfLib.Services.Data;

namespace CX.PdfLib.Implementation.Data
{
    /// <summary>
    /// Base class for bookmarks
    /// </summary>
    public abstract class Bookmark : IBookmark
    {
        /// <summary>
        /// The title of the bookmark
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// Number of the page where the bookmarks starts at
        /// </summary>
        public IList<int> Pages { get; }
        public int StartPage => Pages[0];
        public int EndPage => Pages.Last();

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

        public override bool Equals(object obj) =>
            (obj is Bookmark other) && Equals(other);

        public bool Equals(Bookmark other)
        {
            return CheckEqualProperties(this, other);
        }

        public override int GetHashCode()
        {
            return Title.Concat(Pages.ToString()).GetHashCode();
        }

        public static bool operator == (Bookmark a, Bookmark b)
        {
            return CheckEqualProperties(a, b);
        }

        public static bool operator != (Bookmark a, Bookmark b)
        {
            return !CheckEqualProperties(a, b);
        }

        private static bool CheckEqualProperties(Bookmark a, Bookmark b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Title == b.Title &&
                a.Pages.SequenceEqual(b.Pages);
        }
    }
}
