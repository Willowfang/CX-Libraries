using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.PdfLib.Services.Data
{
    /// <summary>
    /// A bookmark in a tree hierarchy
    /// </summary>
    public interface ILeveledBookmark : IBookmark
    {
        /// <summary>
        /// Level in the tree
        /// </summary>
        public int Level { get; }
    }
}
