using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WF.PdfLib.Services
{
    /// <summary>
    /// Service for pdf/a conversions.
    /// </summary>
    public interface IPdfAConvertService
    {
        /// <summary>
        /// Convert one or more documents to pdf/a.
        /// </summary>
        /// <param name="source">File or folder for conversion. If this is a folder path, all
        /// pdfs inside the folder will be converted.</param>
        /// <param name="destinationDirectory">Directory to save the converted files in.</param>
        /// <param name="cancellation">Cancellation token for the current task.</param>
        /// <returns>True, if conversion was successful.</returns>
        public Task<bool> Convert(FileSystemInfo source, DirectoryInfo destinationDirectory,
            CancellationToken cancellation = default(CancellationToken));
    }
}
