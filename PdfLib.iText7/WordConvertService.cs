using WF.LoggingLib;
using WF.PdfLib.Services;
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace WF.PdfLib.iText7
{   
    /// <summary>
    /// Default implementation of <see cref="IWordConvertService"/>.
    /// </summary>
    public class WordConvertService : LoggingEnabled<WordConvertService>, IWordConvertService
    {
        public WordConvertService(ILogbook logbook): base(logbook) { }

        public async Task<List<FileInfo>> Convert(
            List<WordConvertInput> inputs,
            DirectoryInfo targetDirectory,
            CancellationToken token = default)
        {
            WordConvertWorker worker = new WordConvertWorker(token, logbook.BaseLogbook);
            return await Task.Run(() => worker.Convert(inputs, targetDirectory));
        }

        private class WordConvertWorker : WorkerBase<WordConvertWorker>
        {
            private CancellationToken token;
            internal WordConvertWorker(CancellationToken token, ILogbook logbook) : base(logbook)
            {
                this.token = token;
                PrepareCleanUp();
            }

            internal List<FileInfo> Convert(List<WordConvertInput> inputs, DirectoryInfo targetDirectory)
            {
                Application app = new Application();
                app.Visible = false;
                app.ScreenUpdating = false;

                if (!targetDirectory.Exists) CreatedPaths.Add(targetDirectory);
                targetDirectory.Create();

                logbook.Write($"Conversion started with WordApplication '{app.Name}'.", LogLevel.Debug);

                List<FileInfo> outputPaths = new List<FileInfo>();
                foreach (WordConvertInput input in inputs) {
                    if (!File.Exists(input.InputPath)
                        || (Path.GetExtension(input.InputPath).ToLower() != ".doc")
                            && Path.GetExtension(input.InputPath).ToLower() != ".docx") {
                        throw new IOException("Not a proper input file.");
                    }

                    string outputPath = Path.Combine(targetDirectory.FullName, input.FileName);
                    CreatedPaths.Add(new FileInfo(outputPath));
                    SendConversionToWord(input.InputPath, outputPath, app);

                    outputPaths.Add(new FileInfo(outputPath));

                    if (CheckIfCancelledAndCleanUp(token)) return new List<FileInfo>();
                }

                return outputPaths;
            }

            private void SendConversionToWord(string sourcePath, string outputPath, Application app)
            {
                if (app == null) return;

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
