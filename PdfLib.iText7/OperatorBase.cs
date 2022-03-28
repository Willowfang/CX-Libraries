using CX.LoggingLib;
using CX.PdfLib.Common;
using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CX.PdfLib.iText7
{
    public abstract class OperatorBase<TDerived> : LoggingEnabled<TDerived>
    {
        protected List<FileSystemInfo> CreatedPaths = new();
        protected List<PdfDocument> OpenedDocuments = new();

        public OperatorBase(ILogbook logbook) : base(logbook) { }

        protected void PrepareCleanUp()
        {
            CreatedPaths = new();
            OpenedDocuments = new();
        }

        protected bool CheckIfFileExistsAndCleanUp(FileInfo file)
        {
            return CheckIfFileExistsAndCleanUp(file.FullName);
        }
        protected bool CheckIfFileExistsAndCleanUp(string filePath)
        {
            if (filePath == null || File.Exists(filePath) == false)
            {
                logbook.Write($"File does not exist at {filePath}. Cleaning up.", LogLevel.Warning);
                CleanUp();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if canceled, otherwise false. Performs
        /// <see cref="CleanUp"/> if operation has been canceled.
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        protected bool CheckIfCancelledAndCleanUp(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested)
            {
                logbook.Write($"Cancellation has been requested by user for token {cancellation.GetHashCode()}. Cleaning up.", LogLevel.Debug);
                CleanUp();
                return true;
            }

            return false;
        }

        protected virtual void CleanUp()
        {
            foreach (PdfDocument doc in OpenedDocuments)
            {
                if (doc.IsClosed() == false)
                {
                    doc.Close();
                }
            }

            foreach (FileSystemInfo path in CreatedPaths)
            {
                if (path.Exists)
                {
                    if (path is DirectoryInfo dir)
                        dir.Delete(true);
                    else
                        path.Delete();
                }
            }
        }
    }
}
