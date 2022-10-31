using System.Collections.Generic;
using System.Linq;

namespace WF.PdfLib.Extensions
{
    public static class ListExtensions
    {
        /// <summary>
        /// Get a new range.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="start">Count to start from.</param>
        /// <param name="count">Count of numbers.</param>
        /// <returns>A new list of ints.</returns>
        public static List<int> Range(this List<int> list, int start, int count)
        {
            return Enumerable.Range(start, count).ToList();
        }
    }

    public static class RangeList
    {
        /// <summary>
        /// Create a new list of an int range.
        /// </summary>
        /// <param name="start">Starting number.</param>
        /// <param name="count">Count of numbers.</param>
        /// <returns>A new list of ints with given range.</returns>
        public static List<int> Create(int start, int count)
        {
            return Enumerable.Range(start, count).ToList();
        }
    }
}
