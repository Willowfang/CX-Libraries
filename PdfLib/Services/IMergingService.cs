using CX.PdfLib.Common;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CX.PdfLib.Services
{
    /// <summary>
    /// Service for merging documents together.
    /// </summary>
    public interface IMergingService
    {
        /// <summary>
        /// Merge pdf documents into one document.
        /// </summary>
        /// <param name="sourcePaths">Source files for merging (in sequential order).</param>
        /// <param name="outputPath">Output file path.</param>
        /// <returns>Merged documents' start pages in the new document.</returns>
        public Task<IList<int>> Merge(IList<string> sourcePaths, string outputPath);

        /// <summary>
        /// Merge pdf documents into one document.
        /// </summary>
        /// <param name="sourcePaths">Source files for merging (in sequential order).</param>
        /// <param name="outputPath">Output file path.</param>
        /// <param name="token">Cancellation token for current task.</param>
        /// <returns>Merged documents' start pages in the new document.</returns>
        public Task<IList<int>> Merge(IList<string> sourcePaths, string outputPath, 
            CancellationToken token);

        /// <summary>
        /// Merge pdf documents into one document using provided options.
        /// </summary>
        /// <param name="options">Options for merging.</param>
        /// <returns>Files that were created.</returns>
        public Task<IList<FileSystemInfo>> MergeWithOptions(MergingOptions options);
    }
}
