using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CX.PdfLib.Services
{
    /// <summary>
    /// Convert files to pdf
    /// </summary>
    public interface IWordConvertService
    {
        /// <summary>
        /// Convert a single document
        /// </summary>
        /// <param name="filePath">Path to the document to convert</param>
        /// <param name="outputDirectory">Directory to save document in</param>
        /// <returns>Path to the converted document</returns>
        /// <remarks>The newly created document will be saved with the same name as the
        /// original file (with extension .pdf)</remarks>
        public Task<string> Convert(string filePath, string outputDirectory);
        /// <summary>
        /// Convert a single document asynchronously
        /// </summary>
        /// <param name="filePath">Path to the document to convert</param>
        /// <param name="outputDirectory">Directory to save document in</param>
        /// <param name="token">Token for method cancellation</param>
        /// <returns>Path to the converted document</returns>
        /// <remarks>The newly created document will be saved with the same name as the
        /// original file (with extension .pdf)</remarks>
        public Task<string> Convert(string filePath, string outputDirectory, CancellationToken token);
        /// <summary>
        /// Convert multiple documents
        /// </summary>
        /// <param name="filePaths">Documents to convert</param>
        /// <param name="outputDirectory">Directory to save the documents in</param>
        /// <returns>Paths to converted documents</returns>
        /// // <remarks>Newly created documents will be saved with the same name as the
        /// original files (with extension .pdf)</remarks>
        public Task<IList<string>> Convert(IList<string> filePaths, string outputDirectory);
        /// <summary>
        /// Convert multiple documents asynchronously
        /// </summary>
        /// <param name="filePaths">Documents to convert</param>
        /// <param name="outputDirectory">Directory to save the documents in</param>
        /// <param name="token">Token for method cancellation</param>
        /// <returns>Paths to converted documents</returns>
        /// // <remarks>Newly created documents will be saved with the same name as the
        /// original files (with extension .pdf)</remarks>
        public Task<IList<string>> Convert(IList<string> filePaths, string outputDirectory,
            CancellationToken token);
    }
}
