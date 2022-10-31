using CX.PdfLib.Services.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;

namespace CX.PdfLib.Common
{
    /// <summary>
    /// Options for merging several documents together.
    /// </summary>
    public class MergingOptions
    {
        /// <summary>
        /// Files (and their info) to merge into the product.
        /// </summary>
        public IEnumerable<IMergeInput> Inputs { get; }

        /// <summary>
        /// Fileinfo for the output product.
        /// </summary>
        public FileInfo OutputFile { get; }

        /// <summary>
        /// If true, add page numbers to product.
        /// </summary>
        public bool AddPageNumbers { get; }

        /// <summary>
        /// If true and <see cref="Inputs"/> includes Word-documents, they will be converted to pdf before
        /// merging. Otherwise they will be ignored.
        /// </summary>
        public bool ConvertWordDocuments { get; }

        /// <summary>
        /// <see cref="IProgress{T}"/> instance to report merging progress with.
        /// </summary>
        public IProgress<ProgressReport> Progress { get; set; }

        /// <summary>
        /// Cancellation token for this operation. Is not serialized.
        /// </summary>
        [JsonIgnore]
        public CancellationToken Cancellation { get; set; }

        /// <summary>
        /// Create new options for merging several files together.
        /// </summary>
        /// <param name="inputs">Files to merge (including their info).</param>
        /// <param name="outputFile">File to merge files into.</param>
        /// <param name="addPageNumbers">If true, add page numbers.</param>
        /// <param name="convertWordDocuments">If true, convert Word-documents to pdf before
        /// merging.</param>
        public MergingOptions(IEnumerable<IMergeInput> inputs, FileInfo outputFile,
            bool addPageNumbers, bool convertWordDocuments)
        {
            Inputs = inputs;
            OutputFile = outputFile;
            AddPageNumbers = addPageNumbers;
            ConvertWordDocuments = convertWordDocuments;
            Progress = null;
            Cancellation = CancellationToken.None;
        }
    }
}
