using CX.LoggingLib;
using CX.PdfLib.Services;
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CX.PdfLib.iText7
{   
    /// <summary>
    /// Implementation of <see cref="IWordConvertService"/> service
    /// </summary>
    public class WordConvertService : LoggingEnabled<WordConvertService>, IWordConvertService
    {
        public WordConvertService(ILogbook logbook) : base(logbook) { }

        public async Task<string> Convert(string filePath, string outputDirectory)
        {
            return await Convert(filePath, outputDirectory, CancellationToken.None);
        }
        public async Task<string> Convert(string filePath, string outputDirectory, CancellationToken token)
        {
            IList<string> result = await Convert(new List<string>() { filePath }, outputDirectory, token);
            if (result.Count > 0) return result[0];

            else return null;
        }

        public async Task<IList<string>> Convert(IList<string> filePaths, string outputDirectory)
        {
            return await Convert(filePaths, outputDirectory, CancellationToken.None);
        }
        public async Task<IList<string>> Convert(IList<string> filePaths, string outputDirectory,
            CancellationToken token)
        {
            WordConvertWorker worker = new WordConvertWorker(filePaths, outputDirectory, token, logbook.BaseLogbook);
            return await Task.Run(() => worker.Convert());
        }

        private class WordConvertWorker : OperatorBase<WordConvertWorker>
        {
            private readonly IList<string> filePaths;
            private readonly List<string> converted;
            private readonly CancellationToken token;
            private string outputDirectory;
            private Application app;

            internal WordConvertWorker(IList<string> filePaths, string outputDirectory,
                CancellationToken token, ILogbook logbook) : base(logbook)
            {
                this.filePaths = filePaths;
                this.outputDirectory = outputDirectory;
                this.token = token;
                converted = new List<string>();
            }

            internal IList<string> Convert()
            {
                app = new Application();
                app.Visible = false;
                app.ScreenUpdating = false;

                logbook.Write($"Conversion started with WordApplication '{app.Name}'.", LogLevel.Debug);

                try
                {
                    foreach (string inputPath in filePaths)
                    {
                        if (string.IsNullOrEmpty(inputPath))
                        {
                            converted.Add(inputPath);
                            continue;
                        }
                        if (CheckIfFileDoesNotExistAndCleanUp(inputPath))
                        {
                            throw new ArgumentException($"File at {inputPath} does not exist.");
                        }

                        if (CheckIfCancelledAndCleanUp(token) == true)
                        {
                            return converted;
                        }
                        ExecuteFileConversion(inputPath);
                    }
                }
                catch (Exception e)
                {
                    logbook.Write($"Word conversion failed at {nameof(WordConvertService)}.", LogLevel.Error, e);
                    CleanUp();
                    throw;
                }
                finally
                {
                    app.Quit();
                }

                return converted;
            }

            private void ExecuteFileConversion(string inputPath)
            {
                string ext = Path.GetExtension(inputPath);
                if (ext != ".doc" && ext != ".docx")
                {
                    converted.Add(inputPath);
                    return;
                }

                if (outputDirectory == null)
                    outputDirectory = Path.GetDirectoryName(inputPath);

                if (Directory.Exists(outputDirectory) == false)
                {
                    DirectoryInfo outDir = new DirectoryInfo(outputDirectory);
                    outDir.Create();
                    CreatedPaths.Add(outDir);
                }

                string outputPath = Path.Combine(outputDirectory,
                    Path.GetFileNameWithoutExtension(inputPath) + ".pdf");

                converted.Add(outputPath);
                CreatedPaths.Add(new FileInfo(outputPath));
                SendConversionToWord(inputPath, outputPath);
            }

            private void SendConversionToWord(string sourcePath, string outputPath)
            {
                var doc = app.Documents.Open(sourcePath);
                if (doc != null)
                {
                    doc.ExportAsFixedFormat(outputPath, WdExportFormat.wdExportFormatPDF);
                    doc.Close();
                }
            }
        }
    }
}
