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
    public abstract class OperatorBase
    {
        protected List<FileSystemInfo> CreatedPaths = new();
        protected List<PdfDocument> OpenedDocuments = new();

        protected void PrepareCleanUp()
        {
            CreatedPaths = new();
            OpenedDocuments = new();
        }

        /// <summary>
        /// Returns true if canceled, otherwise false. Performs
        /// <see cref="CleanUp"/> if operation has been canceled.
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        protected bool CancellationCheck(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested)
            {
                CleanUp();
                return true;
            }

            return false;
        }

        protected virtual void CleanUp()
        {
            foreach (FileSystemInfo path in CreatedPaths)
            {
                if (path.Exists)
                {
                    if (path is DirectoryInfo dir)
                        dir.Delete(true);
                    else
                        path.Delete();
                }
                    path.Delete();
            }

            foreach (PdfDocument doc in OpenedDocuments)
            {
                if (doc.IsClosed() == false)
                    doc.Close();
            }
        }
    }
}
