using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.PdfLib.Extensions
{
    /// <summary>
    /// Extension methods for observablecollections.
    /// </summary>
    public static class ObservableCollectionExtensions
    {
        /// <summary>
        /// Remove all instances in the collection matching the predicate.
        /// </summary>
        /// <typeparam name="T">Type of the collection instances.</typeparam>
        /// <param name="collection">Collection to search for.</param>
        /// <param name="condition">Condition for removal.</param>
        public static void RemoveAll<T>(
            this ObservableCollection<T> collection,
            Func<T, bool> condition
        )
        {
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (condition(collection[i]))
                {
                    collection.RemoveAt(i);
                }
            }
        }
    }
}
