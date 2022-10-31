using WF.PdfLib.Services;
using System.IO;
using iText.Kernel.Pdf;
using iText.Forms;
using System;
using System.Threading.Tasks;
using System.Threading;
using WF.LoggingLib;

namespace WF.PdfLib.iText7
{
    /// <summary>
    /// Default implementation for <see cref="ISigningService"/>.
    /// </summary>
    public class SigningService : LoggingEnabled, ISigningService
    {
        /// <summary>
        /// Create a new implementation instance.
        /// </summary>
        /// <param name="logbook">Logging service.</param>
        public SigningService(ILogbook logbook) : base(logbook) { }

        /// <summary>
        /// Remove digital signatures from a document.
        /// </summary>
        /// <param name="source">Document to remove signatures from.</param>
        /// <param name="destination">Path to save the resulting file at.</param>
        /// <returns>An awaitable task.</returns>
        public async Task RemoveSignature(FileInfo source, FileInfo destination)
        {
            await RemoveSignature(source, destination, CancellationToken.None);
        }

        /// <summary>
        /// Remove digital signatures from a document.
        /// </summary>
        /// <param name="source">Document to remove signatures from.</param>
        /// <param name="destination">Path to save the resulting file at.</param>
        /// <param name="token">Cancellation token for the current task.</param>
        /// <returns>An awaitable task.</returns>
        public async Task RemoveSignature(FileInfo source, FileInfo destination, CancellationToken token)
        {
            SignatureWorker worker = new SignatureWorker(source, destination, token, logbook);
            await Task.Run(() => worker.Remove());
        }

        private class SignatureWorker : WorkerBase<SignatureWorker>
        {
            // Provided by constructor
            private readonly FileInfo source;
            private readonly FileInfo destination;
            private readonly CancellationToken token;

            internal SignatureWorker(FileInfo source, FileInfo destination, CancellationToken token,
                ILogbook logbook) : base(logbook)
            {
                this.source = source;
                this.destination = destination;
                this.token = token;
            }

            internal void Remove()
            {
                try
                {
                    if (CheckIfFileDoesNotExistAndCleanUp(source))
                    {
                        throw new ArgumentException($"File at {source.FullName} does not exist.");
                    }

                    if (CheckIfCancelledAndCleanUp(token))
                    {
                        return;
                    }

                    if (destination.Exists == false)
                    {
                        CreatedPaths.Add(destination);
                    }

                    PdfDocument doc = new PdfDocument(new PdfReader(source.FullName), new PdfWriter(destination.FullName));
                    OpenedDocuments.Add(doc);

                    PdfAcroForm form = PdfAcroForm.GetAcroForm(doc, true);
                    form.FlattenFields();
                    doc.Close();
                }
                catch (Exception e)
                {
                    logbook.Write($"Signature removal failed at {nameof(SignatureWorker)}.", LogLevel.Error, e);
                    CleanUp();
                    throw;
                }
            }
        }
    }
}
