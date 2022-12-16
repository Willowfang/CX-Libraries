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
        /// Create extraction worker.
        /// </summary>
        /// <returns>The worker.</returns>
        public IExtractionWorker CreateWorker();
    }
}
