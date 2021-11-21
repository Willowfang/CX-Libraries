using System.Collections.Generic;

namespace CX.PdfLib.Services
{
    /// <summary>
    /// Service for merging pdf-files into one
    /// </summary>
    public interface IMerger
    {
        /// <summary>
        /// Merge pdf documents into one document
        /// </summary>
        /// <param name="sourcePaths">Source files in order of merging</param>
        /// <param name="outputPath">Output file path</param>
        /// <returns>Merged documents' start pages in the new document</returns>
        public IList<int> Merge(IList<string> sourcePaths, string outputPath);
    }
}
