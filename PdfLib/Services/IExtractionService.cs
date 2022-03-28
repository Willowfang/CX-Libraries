using CX.PdfLib.Common;
using CX.PdfLib.Services.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CX.PdfLib.Services
{
    /// <summary>
    /// Extract pages from a pdf
    /// </summary>
    public interface IExtractionService
    {
        /// <summary>
        /// Extract bookmarks from documents into one or more new documents
        /// </summary>
        /// <param name="options">Options for extraction</param>
        /// <returns>A list of extracted files</returns>
        public Task<IList<FileSystemInfo>> Extract(ExtractionOptions options);
    }
}
