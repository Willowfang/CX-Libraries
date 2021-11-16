using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.PdfLib.Services.Data
{
    /// <summary>
    /// A page range in a document set for extraction
    /// </summary>
    public interface IExtractRange
    {
        /// <summary>
        /// Name of the range to extract (without extension)
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Pages to extract
        /// </summary>
        public IList<int> Pages { get; }
    }
}
