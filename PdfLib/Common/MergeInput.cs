using CX.PdfLib.Services.Data;

namespace CX.PdfLib.Common
{
    /// <summary>
    /// Default implementation for <see cref="IMergeInput"/>
    /// </summary>
    public class MergeInput : IMergeInput
    {
        public string FilePath { get; }
        public string Title { get; }
        public int Level { get; }

        public MergeInput(string filePath, string title, int level)
        {
            FilePath = filePath;
            Title = title;
            Level = level;
        }
    }
}
