using CX.LoggingLib;
using CX.PdfLib.Common;
using CX.PdfLib.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PdfLib.PDFTools
{
    public class PdfAConverter : IPdfAConvertService
    {
        private const string pdfToolsLocation = @"C:\Program Files\Tracker Software\PDF Tools\PDFXTools.exe";
        private TypedLogbook<PdfAConverter> logbook;

        public PdfAConverter(ILogbook logbook)
        {
            this.logbook = logbook.CreateTyped<PdfAConverter>();
        }

        /// <summary>
        /// Convert pdfs to pdf/as using PDF-Tools
        /// </summary>
        /// <remarks>Requires Tracker Software PDF-Tools to be installed at default location</remarks>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns>True, if successful</returns>
        /// <exception cref="FileNotFoundException">Thrown, if PDF-Tools executable or source directory is not found</exception>
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

            string commandText = $"/RunTool:showprog=no;showrep=no convertToPDFA \"{source.FullName}\" /Output:folder=\\\"{destinationDirectory.FullName.TrimEnd('\\')}\\\";filename=\\\"%[FileName]\\\";overwrite=yes;showfiles=no";

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
