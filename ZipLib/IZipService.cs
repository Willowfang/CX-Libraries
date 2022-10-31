using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.ZipLib
{
    /// <summary>
    /// Service for compressing files into a zip-file.
    /// </summary>
    public interface IZipService
    {
        /// <summary>
        /// Compress files into a zip-file.
        /// </summary>
        /// <param name="sourceDirectory">Directory to compress files from.</param>
        /// <param name="destinationFile">The resulting file path.</param>
        /// <returns>An awaitable task.</returns>
        public Task Compress(DirectoryInfo sourceDirectory, FileInfo destinationFile);
    }
}
