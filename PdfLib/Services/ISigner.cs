using System.IO;

namespace CX.PdfLib.Services
{
    /// <summary>
    /// Service for manipulating digital signatures
    /// </summary>
    public interface ISigner
    {
        /// <summary>
        /// Remove digital signatures from a document. Output file will be saved in the given directory
        /// with its name formatted as: <paramref name="sourcePath"/>_<paramref name="postFix"/>.pdf
        /// </summary>
        /// <param name="sourcePath">Path of the source file</param>
        /// <param name="destinationDirectory">Directory to save the new file in</param>
        /// <param name="postFix">Postfix for the new file name</param>
        public void RemoveSignature(string sourcePath, DirectoryInfo destinationDirectory, string postFix);
        /// <summary>
        /// Remove digital signatures from a document.
        /// </summary>
        /// <param name="sourcePath">Path of the source file</param>
        /// <param name="outputFile">Output file</param>
        public void RemoveSignature(string sourcePath, FileInfo outputFile);
        /// <summary>
        /// Remove digital signatures from multiple documents. Output files will be saved in the given directory
        /// with their names formatted as: <paramref name="sourcePaths"/>_<paramref name="postFix"/>.pdf
        /// </summary>
        /// <param name="sourcePaths">Paths of the source files</param>
        /// <param name="destinationDirectory">Directory to save the new files in</param>
        /// <param name="postFix">Postfix for new file names</param>
        public void RemoveSignature(string[] sourcePaths, DirectoryInfo destinationDirectory, string postFix);
    }
}
