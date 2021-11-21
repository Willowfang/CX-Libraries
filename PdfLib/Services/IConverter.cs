using System.Collections.Generic;

namespace CX.PdfLib.Services
{
    /// <summary>
    /// Convert files to pdf
    /// </summary>
    public interface IConverter
    {
        /// <summary>
        /// Convert a single document
        /// </summary>
        /// <param name="filePath">Path to the document to convert</param>
        /// <param name="outputDirectory">Directory to save document in</param>
        /// <returns>Path to the converted document</returns>
        /// <remarks>The newly created document will be saved with the same name as the
        /// original file (with extension .pdf)</remarks>
        public string Convert(string filePath, string outputDirectory);
        /// <summary>
        /// Convert multiple documents
        /// </summary>
        /// <param name="filePaths">Documents to convert</param>
        /// <param name="outputDirectory">Directory to save the documents in</param>
        /// <returns>Paths to converted documents</returns>
        /// // <remarks>Newly created documents will be saved with the same name as the
        /// original files (with extension .pdf)</remarks>
        public IList<string> Convert(IList<string> filePaths, string outputDirectory);
    }
}
