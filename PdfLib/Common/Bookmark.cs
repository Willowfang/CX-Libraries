using System;
using System.Collections.Generic;
using System.Linq;
using CX.PdfLib.Extensions;
using CX.PdfLib.Services.Data;

namespace CX.PdfLib.Common
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
        /// <summary>
        /// Create a new bookmark
        /// </summary>
        /// <param name="title">Name of the bookmark</param>
        /// <param name="startPage">Page number for starting point</param>
        /// <param name="pageCount">Number of pages included in the bookmark's range</param>
        /// <exception cref="ArgumentException">Thrown, if <paramref name="pageCount"/> is negative.</exception>
        public Bookmark(string title, int startPage, int pageCount)
        {
            Title = title;
            if (pageCount < 0)
                throw new ArgumentException("Bookmark page count cannot be negative.");
            Pages = new List<int>().Range(startPage, pageCount);
        }

        public override bool Equals(object obj) =>
            obj is Bookmark other && Equals(other);

        public bool Equals(Bookmark other)
        {
            return CheckEqualProperties(this, other);
        }

        public override int GetHashCode()
        {
            return Title.Concat(Pages.ToString()).GetHashCode();
        }

        public static bool operator ==(Bookmark a, Bookmark b)
        {
            return CheckEqualProperties(a, b);
        }

        public static bool operator !=(Bookmark a, Bookmark b)
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
