using CX.PdfLib.Services.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.PdfLib.Services
{
    /// <summary>
    /// Extract pages from a pdf
    /// </summary>
    public interface IExtractor
    {
        /// <summary>
        /// Extract multiple page ranges into separate files
        /// </summary>
        /// <param name="sourceFile">Path of the file to extract from</param>
        /// <param name="destDirectory">Directory to extract the files to</param>
        /// <param name="ranges">Ranges to extract</param>
        void Extract(string sourceFile, DirectoryInfo destDirectory, IEnumerable<ILeveledBookmark> ranges);
        /// <summary>
        /// Extract multiple page ranges into one file
        /// </summary>
        /// <param name="sourceFile">File to extract from</param>
        /// <param name="destFile">File to extract into</param>
        /// <param name="ranges">Page ranges to extract</param>
        void Extract(string sourceFile, FileInfo destFile, IEnumerable<ILeveledBookmark> ranges);
    }
}
