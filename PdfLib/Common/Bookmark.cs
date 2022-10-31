using System;
using System.Collections.Generic;
using System.Linq;
using CX.PdfLib.Extensions;
using CX.PdfLib.Services.Data;

namespace CX.PdfLib.Common
{
    /// <summary>
    /// Base class for bookmarks.
    /// </summary>
    public abstract class Bookmark : IBookmark
    {
        /// <summary>
        /// The title of the bookmark.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// A list of all the pages contained in the bookmark.
        /// </summary>
        public IList<int> Pages { get; }

        /// <summary>
        /// Start page number of bookmark range.
        /// </summary>
        public int StartPage => Pages[0];

        /// <summary>
        /// End page number of the bookmark range.
        /// </summary>
        public int EndPage => Pages.Last();

        /// <summary>
        /// Create a new bookmark instance.
        /// </summary>
        /// <param name="title">Name of the bookmark.</param>
        /// <param name="pages">Pages contained in the bookmark range.</param>
        public Bookmark(string title, IList<int> pages)
        {
            Title = title;
            Pages = pages;
        }

        /// <summary>
        /// Create a new bookmark instance.
        /// </summary>
        /// <param name="title">Name of the bookmark.</param>
        /// <param name="startPage">Bookmark range starting page.</param>
        /// <param name="pageCount">Number of pages included in the bookmark's range.</param>
        /// <exception cref="ArgumentException">Thrown, if <paramref name="pageCount"/> is negative.</exception>
        public Bookmark(string title, int startPage, int pageCount)
        {
            Title = title;
            if (pageCount < 0)
                throw new ArgumentException("Bookmark page count cannot be negative.");
            Pages = new List<int>().Range(startPage, pageCount);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) =>
            obj is Bookmark other && Equals(other);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Bookmark other)
        {
            return CheckEqualProperties(this, other);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Title.Concat(Pages.ToString()).GetHashCode();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Bookmark a, Bookmark b)
        {
            return CheckEqualProperties(a, b);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Bookmark a, Bookmark b)
        {
            return !CheckEqualProperties(a, b);
        }

        /// <summary>
        /// Check for equal properties between instances.
        /// <para>
        /// Checks for the title and page range of instances. If both are equal, returns true.
        /// </para>
        /// </summary>
        /// <param name="a">First bookmark.</param>
        /// <param name="b">Second bookmark.</param>
        /// <returns></returns>
        private static bool CheckEqualProperties(Bookmark a, Bookmark b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Title == b.Title &&
                a.Pages.SequenceEqual(b.Pages);
        }
    }
}
