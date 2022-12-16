using WF.PdfLib.Services.Data;

namespace WF.PdfLib.Common
{
    /// <summary>
    /// Default implementation for <see cref="IMergeInput"/>.
    /// </summary>
    public class MergeInput : IMergeInput
    {
        /// <summary>
        /// Path of the file to merge.
        /// </summary>
        public string? FilePath { get; }

        /// <summary>
        /// Title of the bookmark to create when merging.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Bookmarks level in the bookmark tree.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Create a new merge info for merging a document into the final product.
        /// </summary>
        /// <param name="filePath">Path of the file to merge. If null, this is a title.</param>
        /// <param name="title">Name to create in the bookmark tree.</param>
        /// <param name="level">Level of this bookmark in the final bookmark tree.</param>
        public MergeInput(string? filePath, string title, int level)
        {
            FilePath = filePath;
            Title = title;
            Level = level;
        }
    }
}
