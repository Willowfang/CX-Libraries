using CX.PdfLib.Services;
using Microsoft.Office.Interop.Word;
using System.Collections.Generic;
using System.IO;

namespace CX.PdfLib.iText7
{
    /// <summary>
    /// Implementation of <see cref="IConverter"/> service
    /// </summary>
    public class ConverterWord : IConverter
    {
        public string Convert(string filePath, string outputDirectory)
        {
            IList<string> result = Convert(new List<string>() { filePath }, outputDirectory);
            if (result.Count > 0) return result[0];

            else return null;
        }

        public IList<string> Convert(IList<string> filePaths, string outputDirectory)
        {
            var app = new Application();
            app.Visible = false;
            app.ScreenUpdating = false;
            List<string> converted = new List<string>();

            foreach (string inputPath in filePaths)
            {
                var ext = Path.GetExtension(inputPath);
                if (ext != ".doc" && ext != ".docx")
                {
                    converted.Add(inputPath);
                    continue;
                }

                if (outputDirectory == null)
                    outputDirectory = Path.GetDirectoryName(inputPath);
                string outputPath = Path.Combine(outputDirectory,
                    Path.GetFileNameWithoutExtension(inputPath) + ".pdf");
                ExecuteConvert(app, inputPath, outputPath);
                converted.Add(outputPath);
            }

            app.Quit();

            return converted;
        }

        private void ExecuteConvert(Application wordApp, string sourcePath, string outputPath)
        {
            var doc = wordApp.Documents.Open(sourcePath);
            if (doc != null)
            {
                doc.ExportAsFixedFormat(outputPath, WdExportFormat.wdExportFormatPDF);
                doc.Close();
            }
        }
    }
}
