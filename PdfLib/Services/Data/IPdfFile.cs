using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.PdfLib.Services.Data
{
    /// <summary>
    /// Information of a file in pdf-format
    /// </summary>
    public interface IPdfFile
    {
        /// <summary>
        /// Title of the file
        /// </summary>
        string Title { get; }
        /// <summary>
        /// Path to the file
        /// </summary>
        string FilePath { get; }
        /// <summary>
        /// True, if the file has been selected
        /// </summary>
        bool IsSelected { get; set; }
    }
}
