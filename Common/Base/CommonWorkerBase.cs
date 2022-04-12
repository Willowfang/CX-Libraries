using CX.LoggingLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CX.Common.Base
{
    public abstract class CommonWorkerBase<TDerived> : LoggingEnabled<TDerived>
    {
        protected List<FileSystemInfo> CreatedPaths = new();

        public CommonWorkerBase(ILogbook logbook) : base(logbook) { }

        protected virtual void PrepareCleanUp()
        {
            CreatedPaths = new();
        }

        protected virtual bool CheckIfFileDoesNotExistAndCleanUp(FileInfo file)
        {
            return CheckIfFileDoesNotExistAndCleanUp(file.FullName);
        }
        protected virtual bool CheckIfFileDoesNotExistAndCleanUp(string filePath)
        {
            if (filePath == null || File.Exists(filePath) == false)
            {
                logbook.Write($"File does not exist at {filePath}. Cleaning up.", LogLevel.Warning);
                CleanUp();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if canceled, otherwise false. Performs
        /// <see cref="CleanUp"/> if operation has been canceled.
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        protected virtual bool CheckIfCancelledAndCleanUp(CancellationToken cancellation)
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
