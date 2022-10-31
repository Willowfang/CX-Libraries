using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WF.PdfLib.Services
{
    /// <summary>
    /// Service for converting Word-files to pdf.
    /// </summary>
    public interface IWordConvertService
    {
        /// <summary>
        /// Convert a single document.
        /// </summary>
        /// <param name="filePath">Path of the document to convert.</param>
        /// <param name="outputDirectory">Directory to save the converted document in.</param>
        /// <returns>Path to the converted document.</returns>
        /// <remarks>The newly created document will be saved with the same name as the
        /// original file (with extension .pdf).</remarks>
        public Task<string> Convert(string filePath, string outputDirectory);

        /// <summary>
        /// Convert a single document asynchronously.
        /// </summary>
        /// <param name="filePath">Path of the document to convert.</param>
        /// <param name="outputDirectory">Directory to save the converted document in.</param>
        /// <param name="token">Cancellation token for the current task.</param>
        /// <returns>Path to the converted document.</returns>
        /// <remarks>The newly created document will be saved with the same name as the
        /// original file (with extension .pdf).</remarks>
        public Task<string> Convert(string filePath, string outputDirectory, CancellationToken token);

        /// <summary>
        /// Convert multiple documents.
        /// </summary>
        /// <param name="filePaths">Documents to convert.</param>
        /// <param name="outputDirectory">Directory to save the converted documents in.</param>
        /// <returns>Paths to converted documents.</returns>
        /// <remarks>Newly created documents will be saved with the same name as the
        /// original files (with extension .pdf)</remarks>
        public Task<IList<string>> Convert(IList<string> filePaths, string outputDirectory);

        /// <summary>
        /// Convert multiple documents asynchronously.
        /// </summary>
        /// <param name="filePaths">Documents to convert.</param>
        /// <param name="outputDirectory">Directory to save the converted documents in.</param>
        /// <param name="token">Cancellation token for the current task.</param>
        /// <returns>Paths to converted documents.</returns>
        /// <remarks>Newly created documents will be saved with the same name as the
        /// original files (with extension .pdf)</remarks>
        public Task<IList<string>> Convert(IList<string> filePaths, string outputDirectory,
            CancellationToken token);
    }
}
