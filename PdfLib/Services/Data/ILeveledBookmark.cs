namespace CX.PdfLib.Services.Data
{
    /// <summary>
    /// A bookmark in a tree hierarchy
    /// </summary>
    public interface ILeveledBookmark : IBookmark
    {
        /// <summary>
        /// Level in the tree
        /// </summary>
        public int Level { get; }
    }
}
