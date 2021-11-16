using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.PdfLib.Services.Data
{
    /// <summary>
    /// Info on a file or title to be used in a document merge
    /// </summary>
    public interface IMergeInput
    {
        /// <summary>
        /// Path to the file to merge. Null, if just a title.
        /// </summary>
        public string FilePath { get; }
        /// <summary>
        /// Title of the bookmark to add
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// Level of the bookmark in the bookmark tree
        /// </summary>
        public int Level { get; }
    }
}
