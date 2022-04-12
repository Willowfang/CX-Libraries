using CX.PdfLib.Services;
using System.IO;
using iText.Kernel.Pdf;
using iText.Forms;
using System;
using System.Threading.Tasks;
using System.Threading;
using CX.LoggingLib;

namespace CX.PdfLib.iText7
{
    /// <summary>
    /// Default implementation for ISigner service
    /// </summary>
    public class SigningService : LoggingEnabled, ISigningService
    {
        public SigningService(ILogbook logbook) : base(logbook) { }
        public async Task RemoveSignature(FileInfo source, FileInfo destination)
        {
            await RemoveSignature(source, destination, CancellationToken.None);
        }

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
