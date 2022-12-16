using WF.PdfLib.Services.Data;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;

namespace WF.PdfLib.Common
{
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
        /// Files to extract bookmarks from and the bookmarks to extract from them.
        /// </summary>
        public IEnumerable<FileAndExtractables> Files { get; }

        /// <summary>
        /// File or folder to extract the bookmarks into.
        /// </summary>
        public FileSystemInfo Destination { get; }

        /// <summary>
        /// Cancellation token for this operation. Is ignored when serializing (is not serializable).
        /// </summary>
        [JsonIgnore]
        public CancellationToken Cancellation { get; set; }

        /// <summary>
        /// Chosen option for dealing with annotations.
        /// </summary>
        public AnnotationOption Annotations { get; set; }

        /// <summary>
        /// The users, whose annotations will be removed from the products.
        /// </summary>
        public IEnumerable<string> AnnotationUsersToRemove { get; set; }

        public bool ConvertToPdfA { get; set; }

        /// <summary>
        /// Create new options for extraction.
        /// </summary>
        /// <param name="files">Files and bookmarks to extract.</param>
        /// <param name="destination">Folder to extract to.</param>
        public ExtractionOptions(IEnumerable<FileAndExtractables> files, FileSystemInfo destination)
        {
            Files = files;
            Destination = destination;
            Cancellation = CancellationToken.None;
            Annotations = AnnotationOption.Keep;
            AnnotationUsersToRemove = new List<string>();
        }
    }
}
