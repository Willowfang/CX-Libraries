using WF.PdfLib.Common;
using WF.PdfLib.Services.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WF.PdfLib.Services
{
    /// <summary>
    /// Service for extracting part of documents.
    /// </summary>
    public interface IExtractionService
    {
        /// <summary>
        /// Extract bookmarks from documents into one or more new documents.
        /// </summary>
        /// <param name="options">Options for extraction.</param>
        /// <returns>A list of product files.</returns>
        public Task<IList<FileSystemInfo>> Extract(ExtractionOptions options);
    }
}
