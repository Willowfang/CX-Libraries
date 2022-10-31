using WF.PdfLib.Services.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace WF.PdfLib.Common
{
    /// <summary>
    /// Event arguments passed when converting to pdf/a.
    /// </summary>
    public class PdfAConversionEventArgs : EventArgs
    {
        /// <summary>
        /// True, if conversion was not successful.
        /// </summary>
        public bool WasFaulted { get; set; }
        /// <summary>
        /// Path of the converted file.
        /// </summary>
        public string Path { get; set; }
    }

    /// <summary>
    /// A class for holding information about a specific file and the bookmarks to extract from it.
    /// </summary>
    public class FileAndExtractables
    {
        /// <summary>
        /// Path of the file containing the bookmarks.
        /// </summary>
        public string FilePath { get; }
        /// <summary>
        /// Bookmarks to extract from the document.
        /// </summary>
        public IEnumerable<ILeveledBookmark> Extractables { get; }

        /// <summary>
        /// Create a new instance containing information about a file and bookmarks to extract from it.
        /// </summary>
        /// <param name="filePath">Path to the file in question.</param>
        /// <param name="extractables">´Bookmarks to extract from this file.</param>
        public FileAndExtractables(string filePath, IEnumerable<ILeveledBookmark> extractables)
        {
            FilePath = filePath;
            Extractables = extractables;
        }
    }

    /// <summary>
    /// Options to use when extracting bookmarks from a file.
    /// </summary>
    public class ExtractionOptions
    {
        /// <summary>
        /// This event is invoked every time a file has been converted to pdf/a. 
        /// </summary>
        public event EventHandler<PdfAConversionEventArgs> PdfAConversionFinished;

        /// <summary>
        /// Files to extract bookmarks from and the bookmarks to extract from them.
        /// </summary>
        public IEnumerable<FileAndExtractables> Files { get; }

        /// <summary>
        /// File or folder to extract the bookmarks into.
        /// </summary>
        public FileSystemInfo Destination { get; }

        /// <summary>
        /// <see cref="IProgress{T}"/> instance to report the extraction progress.
        /// </summary>
        public IProgress<ProgressReport> Progress { get; set; }

        /// <summary>
        /// Cancellation token for this operation. Is ignored when serializing (is not serializable).
        /// </summary>
        [JsonIgnore]
        public CancellationToken Cancellation { get; set; }

        /// <summary>
        /// If true, results will be converted to pdf/a.
        /// </summary>
        public bool PdfA { get; set; }

        /// <summary>
        /// Chosen option for dealing with annotations.
        /// </summary>
        public AnnotationOption Annotations { get; set; }

        /// <summary>
        /// The users, whose annotations will be removed from the products.
        /// </summary>
        public IEnumerable<string> AnnotationUsersToRemove { get; set; }

        /// <summary>
        /// Create new options for extraction.
        /// </summary>
        /// <param name="files">Files and bookmarks to extract.</param>
        /// <param name="destination">File or folder to extract to.</param>
        public ExtractionOptions(IEnumerable<FileAndExtractables> files, FileSystemInfo destination)
        {
            Files = files;
            Destination = destination;
            Progress = null;
            Cancellation = CancellationToken.None;
            PdfA = false;
            Annotations = AnnotationOption.Keep;
            AnnotationUsersToRemove = new List<string>();
        }

        /// <summary>
        /// Invokes <see cref="PdfAConversionFinished"/> event handler.
        /// </summary>
        /// <param name="e">Arguments for event.</param>
        public virtual void OnPdfAConversionFinished(PdfAConversionEventArgs e)
        {
            EventHandler<PdfAConversionEventArgs> handler = PdfAConversionFinished;
            handler?.Invoke(this, e);
        }
    }
}
