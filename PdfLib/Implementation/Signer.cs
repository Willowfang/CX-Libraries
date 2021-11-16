using CX.PdfLib.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CX.PdfLib.Extensions;
using iText.Kernel.Pdf;
using iText.Forms;

namespace CX.PdfLib.Implementation
{
    /// <summary>
    /// Default implementation for ISigner service
    /// </summary>
    public class Signer : ISigner
    {
        public void RemoveSignature(string sourcePath, DirectoryInfo destinationDirectory, string postFix)
        {
            ExecuteRemove(sourcePath, GetOutputPath(sourcePath, destinationDirectory, postFix));
        }

        public void RemoveSignature(string sourcePath, FileInfo outputFile)
        {
            ExecuteRemove(sourcePath, outputFile.FullName);
        }

        public void RemoveSignature(string[] sourcePaths, DirectoryInfo destinationDirectory, string postFix)
        {
            foreach (string source in sourcePaths)
            {
                ExecuteRemove(source, GetOutputPath(source, destinationDirectory, postFix));
            }
        }

        private void ExecuteRemove(string sourcePath, string outputPath)
        {
            PdfDocument doc = new PdfDocument(new PdfReader(sourcePath), new PdfWriter(outputPath));
            PdfAcroForm form = PdfAcroForm.GetAcroForm(doc, true);
            form.FlattenFields();
            doc.Close();
        }

        private string GetOutputPath(string sourceFile, DirectoryInfo directory, string postFix)
        {
            string fileName = string.Concat(Path.GetFileNameWithoutExtension(sourceFile), "_", postFix, ".pdf");
            return Path.Combine(directory.FullName, fileName);
        }
    }
}
