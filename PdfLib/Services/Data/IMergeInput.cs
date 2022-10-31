namespace WF.PdfLib.Services.Data
{
    /// <summary>
    /// Info on a file or title to be used in a document merge.
    /// </summary>
    public interface IMergeInput : ILeveledItem
    {
        /// <summary>
        /// Path to the file to merge. Null, if just a title.
        /// </summary>
        public string FilePath { get; }
        /// <summary>
        /// Title of the bookmark to add.
        /// </summary>
        public string Title { get; }
    }
}
