using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WF.PdfLib.Common;

namespace WF.PdfLib.Services
{
    public class BookmarkOrFileExtractedEventArgs : EventArgs
    {
        public string Name { get; }
        public bool IsDone { get; }

        public BookmarkOrFileExtractedEventArgs(string name, bool isDone = false)
        {
            Name = name;
            IsDone = isDone;
        }
    }

    public class FileMergedEventArgs : EventArgs
    {
        /// <summary>
        /// Path of the file that was merged.
        /// </summary>
        public string FilePath { get; }

        public bool IsDone { get; }

        public FileMergedEventArgs(string filePath, bool isDone = false)
        {
            FilePath = filePath;
            IsDone = isDone;
        }
    }

    public class FilesConvertedToPdfAEventArgs : EventArgs
    {
        /// <summary>
        /// True, if there was an error when converting.
        /// </summary>
        public bool WasFaulted { get; }

        /// <summary>
        /// Path of the destination directory.
        /// </summary>
        public string DirectoryPath { get; }

        public FilesConvertedToPdfAEventArgs(bool wasFaulted, string filePath)
        {
            WasFaulted = wasFaulted;
            DirectoryPath = filePath;
        }
    }

    public interface IExtractionWorker
    {
        public event EventHandler<BookmarkOrFileExtractedEventArgs> BookmarkOrFileExtracted;

        public event EventHandler<FileMergedEventArgs> FileMerged;

        public event EventHandler<FilesConvertedToPdfAEventArgs> FilesConverted;

        /// <summary>
        /// Extract bookmarks from documents.
        /// </summary>
        /// <param name="options">Options for extraction.</param>
        /// <returns>The directory where the files where created. If a <see cref="FileInfo"/> was
        /// provided as destination in the options, this will be a temporary directory under
        /// the parent directory of the given file. If a <see cref="DirectoryInfo"/> was provided,
        /// it will be returned here.</returns>
        public Task<DirectoryInfo> Extract(ExtractionOptions options);
    }
}
