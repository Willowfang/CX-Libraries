using WF.LoggingLib;
using iText.Forms;
using iText.Kernel.Pdf;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace WF.PdfLib.iText7
{
    /// <summary>
    /// Various utility methods for pdf handling.
    /// </summary>
    internal class Utilities : LoggingEnabled<Utilities>
    {
        /// <summary>
        /// Create a new utility instance.
        /// </summary>
        /// <param name="logbook">Logging service.</param>
        internal Utilities(ILogbook logbook) : base(logbook) { }

        /// <summary>
        /// Flatten a pdf document.
        /// </summary>
        /// <param name="doc">Document to flatten.</param>
        public void Flatten(PdfDocument doc)
        {
            logbook.Write($"Flattening form fields (including signatures).", LogLevel.Debug);

            PdfAcroForm form = PdfAcroForm.GetAcroForm(doc, true);
            form.FlattenFields();
        }

        public DirectoryInfo CreateTemporaryDirectory()
        {
            string temporaryDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            DirectoryInfo temporaryDirectory = new DirectoryInfo(temporaryDirectoryPath);

            try
            {
                temporaryDirectory.Create();
            }
            catch (Exception ex)
            {
                logbook.Write($"Temporary extraction directory creation failed (DirectoryInfo.Create() with a path of {temporaryDirectoryPath}).", LogLevel.Error, ex);
                throw;
            }

            return temporaryDirectory;
        }
    }
}
