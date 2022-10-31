using CX.LoggingLib;

namespace CX.Common.Base
{
    /// <summary>
    /// An abstract base class for workers performing various tasks with
    /// common functionalities (such as cleaning up after cancellation).
    /// </summary>
    /// <typeparam name="TDerived">Worker that derives from this base class (for logging purposes)</typeparam>
    public abstract class CommonWorkerBase<TDerived> : LoggingEnabled<TDerived>
    {
        /// <summary>
        /// File and folder paths created while executing the assigned task.
        /// </summary>
        protected List<FileSystemInfo> CreatedPaths = new();

        /// <summary>
        /// Create a new base worker.
        /// </summary>
        /// <param name="logbook">Logging service</param>
        public CommonWorkerBase(ILogbook logbook) : base(logbook) { }

        /// <summary>
        /// Makes preparations for cleaning up before running the task. 
        /// Replaces <see cref="CreatedPaths"/> with a new list.
        /// </summary>
        protected virtual void PrepareCleanUp()
        {
            CreatedPaths = new();
        }

        /// <summary>
        /// See <see cref="CheckIfFileDoesNotExistAndCleanUp(string)"/>. Sends full name of the file info
        /// to said method.
        /// </summary>
        /// <param name="file">Check for the existence of this file.</param>
        /// <returns>True, if the file does not exist.</returns>
        protected virtual bool CheckIfFileDoesNotExistAndCleanUp(FileInfo file)
        {
            return CheckIfFileDoesNotExistAndCleanUp(file.FullName);
        }

        /// <summary>
        /// Check if a file exists. If the file does not exist, clean up and delete all files
        /// created during the performance of the task.
        /// </summary>
        /// <param name="filePath">File to check for.</param>
        /// <returns>True, if the file does not exist.</returns>
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
        /// <param name="cancellation">Token for cancellation checking.</param>
        /// <returns>True, if canceled.</returns>
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

        /// <summary>
        /// Perform clean up. Delete all files and folders in <see cref="CreatedPaths"/>.
        /// </summary>
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
