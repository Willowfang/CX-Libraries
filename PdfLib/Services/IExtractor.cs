using CX.PdfLib.Common;
using CX.PdfLib.Services.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace CX.PdfLib.Services
{
    /// <summary>
    /// Extract pages from a pdf
    /// </summary>
    public interface IExtractor
    {
        /// <summary>
        /// Extract multiple page ranges into separate files
        /// </summary>
        /// <param name="sourceFile">Path of the file to extract from</param>
        /// <param name="destDirectory">Directory to extract the files to</param>
        /// <param name="ranges">Ranges to extract</param>
        /// <param name="progress">Progress reporter for async calls</param>
        /// <param name="cancellation">Token for process cancellation</param>
        public IList<FileSystemInfo> Extract(string sourceFile, DirectoryInfo destDirectory, 
            IEnumerable<ILeveledBookmark> ranges, IProgress<ProgressReport> progress = null, 
            CancellationToken cancellation = default(CancellationToken));
        /// <summary>
        /// Extract multiple page ranges into one file
        /// </summary>
        /// <param name="sourceFile">File to extract from</param>
        /// <param name="destFile">File to extract into</param>
        /// <param name="ranges">Page ranges to extract</param>
        /// <param name="progress">Progress reporter for async calls</param>
        /// <param name="cancellation">Token for process cancellation</param>
        public void Extract(string sourceFile, FileInfo destFile, IEnumerable<ILeveledBookmark> ranges,
            IProgress<ProgressReport> progress = null, CancellationToken cancellation = default(CancellationToken));
    }
}
