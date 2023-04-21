using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WF.PdfLib.Services
{
    public class WordConvertInput
    {
        public string InputPath { get; }
        public string FileName { get; }

        public WordConvertInput(
            string inputPath, 
            string fileName)
        {
            InputPath = inputPath;
            FileName = fileName;
        }
    }
    /// <summary>
    /// Service for converting Word-files to pdf.
    /// </summary>
    public interface IWordConvertService
    {
        /// <summary>
        /// Convert word documents.
        /// </summary>
        /// <param name="inputs">Input files.</param>
        /// <param name="targetDirectory">Directory to convert to.</param>
        /// <param name="token">Token for canceling operation.</param>
        /// <returns>A list of converted file filepaths.</returns>
        public Task<List<FileInfo>> Convert(
            List<WordConvertInput> inputs, 
            DirectoryInfo targetDirectory,
            CancellationToken token = default);
    }
}
