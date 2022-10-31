using CX.Common.Base;
using CX.LoggingLib;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.ZipLib.Framework
{
    /// <summary>
    /// Implementation for <see cref="IZipService"/>.
    /// </summary>
    public class ZipService : LoggingEnabled, IZipService
    {
        /// <summary>
        /// Create a new implementation instance.
        /// </summary>
        /// <param name="logbook">Logging service.</param>
        public ZipService(ILogbook logbook) : base(logbook) { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="destinationFile"></param>
        /// <returns></returns>
        public async Task Compress(DirectoryInfo sourceDirectory, FileInfo destinationFile)
        {
            ZipCompressWorker worker = new ZipCompressWorker(sourceDirectory, destinationFile, logbook);
            await Task.Run(() => worker.Compress());
        }

        private class ZipCompressWorker : CommonWorkerBase<ZipCompressWorker>
        {
            private readonly DirectoryInfo sourceDirectory;
            private readonly FileInfo destinationFile;

            internal ZipCompressWorker(DirectoryInfo sourceDirectory, FileInfo destinationFile, 
                ILogbook logbook) : base(logbook)
            {
                this.sourceDirectory = sourceDirectory;
                this.destinationFile = destinationFile;
            }

            internal void Compress()
            {
                try
                {
                    ZipFile.CreateFromDirectory(sourceDirectory.FullName, destinationFile.FullName,
                        CompressionLevel.Fastest, false);
                }
                catch (Exception e)
                {
                    logbook.Write($"Zipfile creation failed.", LogLevel.Error, e);
                    throw;
                }
            }
        }
    }
}
