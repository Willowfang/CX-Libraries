using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CX.PdfLib.Services
{
    /// <summary>
    /// Convert a regular pdf into pdf/a
    /// </summary>
    public interface IPdfAConvertService
    {
        /// <summary>
        /// Convert to pdf/a
        /// </summary>
        /// <param name="source">File or folder</param>
        /// <param name="destinationDirectory"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public Task<bool> Convert(FileSystemInfo source, DirectoryInfo destinationDirectory,
            CancellationToken cancellation = default(CancellationToken));
    }
}
