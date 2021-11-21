using CX.PdfLib.Services.Data;
using System.Collections.Generic;

namespace CX.PdfLib.Services
{
    public interface IManipulator : IExtractor, IConverter, IMerger,
        IBookmarker, ISigner
    {
        /// <summary>
        /// Merge documents into one pdf. Create top-level bookmarks for merged
        /// files and assign them given titles. Retain all bookmarks from original documents
        /// with adjusted destination pages.
        /// </summary>
        /// <param name="inputs">Inputs for the merge, includes merging files
        /// and titles</param>
        /// <param name="outputPath">Output file path</param>
        public void MergeWithBookmarks(IList<IMergeInput> inputs, string outputPath);
    }
}
