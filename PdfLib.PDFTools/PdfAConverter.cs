using CX.LoggingLib;
using CX.PdfLib.Common;
using CX.PdfLib.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PdfLib.PDFTools
{
    /// <summary>
    /// A class for performing pdf/a conversions. Implements <see cref="IPdfAConvertService"/>.
    /// </summary>
    public class PdfAConverter : IPdfAConvertService
    {
        private const string pdfToolsLocation = @"C:\Program Files\Tracker Software\PDF Tools\PDFXTools.exe";
        private TypedLogbook<PdfAConverter> logbook;

        /// <summary>
        /// Create a new instance of pdf/a converter.
        /// </summary>
        /// <param name="logbook">Logging service.</param>
        public PdfAConverter(ILogbook logbook)
        {
            this.logbook = logbook.CreateTyped<PdfAConverter>();
        }

        /// <summary>
        /// Convert pdfs to pdf/as using PDF-Tools.
        /// </summary>
        /// <remarks>Requires Tracker Software PDF-Tools to be installed at default location.</remarks>
        /// <param name="source">Source to convert from.</param>
        /// <param name="destinationDirectory">Directory to save products in.</param>
        /// <param name="cancellation">Cancellation token for current task.</param>
        /// <returns>True, if successful</returns>
        /// <exception cref="FileNotFoundException">Thrown, if PDF-Tools executable or source directory is 
        /// not found.</exception>
        public async Task<bool> Convert(FileSystemInfo source, DirectoryInfo destinationDirectory,
            CancellationToken cancellation = default(CancellationToken))
        {
            if (cancellation.IsCancellationRequested) return true;

            logbook.Write($"Converting from {source.Name} to {destinationDirectory.Name}.", LogLevel.Information);

            if (!File.Exists(pdfToolsLocation))
                throw new FileNotFoundException(pdfToolsLocation);
            if (!source.Exists)
            {
                if (source is DirectoryInfo)
                {
                    throw new DirectoryNotFoundException(source.FullName);
                }
                else
                {
                    throw new FileNotFoundException(source.FullName);
                }
            }
            if (!destinationDirectory.Exists)
                destinationDirectory.Create();

            string tempDir = Path.Combine(Path.GetTempPath(), Path.GetTempFileName()).TrimEnd('\\');
            Directory.CreateDirectory(tempDir);

            string commandText = $"/RunTool:showprog=no;showrep=no convertToPDFA \"{source.FullName}\" /Output:folder=\\\"{tempDir}\\\";filename=\\\"%[FileName]\\\";overwrite=yes;showfiles=no";

            foreach (string file in Directory.GetFiles(tempDir))
            {
                FileInfo tempFile = new FileInfo(file);
                tempFile.CopyTo(Path.Combine(destinationDirectory.FullName, tempFile.Name));
            }

            Directory.Delete(tempDir, true);

            logbook.Write($"Command: {commandText}", LogLevel.Debug);

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            info.FileName = pdfToolsLocation;
            info.Arguments = commandText;
            process.StartInfo = info;
            process.Start();

            Task processTask = process.WaitForExitAsync(cancellation);

            try
            {
                await processTask;
            }
            catch (OperationCanceledException)
            {
                return true;
            }
            catch (Exception ex)
            {
                logbook.Write($"PDF Tools encountered an error.", LogLevel.Error, ex);
                return false;
            }

            if (processTask.IsFaulted || (process.HasExited && process.ExitCode != 0))
            {
                logbook.Write($"PDF Tools process is at faulted state or its exit code is not 0.", LogLevel.Error);
                return false;
            }

            if (cancellation.IsCancellationRequested)
            {
                if (process.HasExited == false)
                    process.Kill();
            }

            return true;
        }
    }
}
