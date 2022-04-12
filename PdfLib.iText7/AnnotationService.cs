using CX.LoggingLib;
using CX.PdfLib.Services;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Canvas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IOPath = System.IO.Path;

namespace CX.PdfLib.iText7
{
    public class AnnotationService : LoggingEnabled, IAnnotationService
    {
        public AnnotationService(ILogbook logbook) : base(logbook) 
        { }

        public async Task<IEnumerable<string>> GetTitles(string inputPath)
        {
            return await GetTitles(inputPath, CancellationToken.None);
        }
        public async Task<IEnumerable<string>> GetTitles(string inputPath,
            CancellationToken token)
        {
            AnnotationTitleWorker worker = new AnnotationTitleWorker(inputPath,
                token, logbook);
            return await Task.Run(() => worker.GetTitles());
        }

        public async Task RemoveAll(string inputPath)
        {
            await RemoveByTitle(null, inputPath);
        }
        public async Task RemoveAll(string inputPath, CancellationToken token)
        {
            await RemoveByTitle(null, inputPath, token);
        }

        public async Task RemoveByTitle(IEnumerable<string> titles, string inputPath)
        {
            await RemoveByTitle(titles, inputPath, CancellationToken.None);
        }
        public async Task RemoveByTitle(IEnumerable<string> titles, string inputPath,
            CancellationToken token)
        {
            AnnotationRemovalWorker worker = new AnnotationRemovalWorker(titles, inputPath,
                token, logbook);
            await Task.Run(() => worker.Remove());
        }

        public async Task FlattenRedactions(string inputPath)
        {
            await FlattenRedactions(inputPath, CancellationToken.None);
        }
        public async Task FlattenRedactions(string inputPath, CancellationToken token)
        {
            RedactionFlatteningWorker worker = new RedactionFlatteningWorker(inputPath, token, logbook);
            await Task.Run(() => worker.Flatten());
        }

        internal async Task RemoveAll(PdfDocument doc, CancellationToken token)
        {
            await RemoveByTitle(null, doc, token);
        }
        internal async Task RemoveByTitle(IEnumerable<string> titles, PdfDocument doc,
            CancellationToken token)
        {
            AnnotationRemovalWorker worker = new AnnotationRemovalWorker(titles, doc,
                token, logbook);
            await Task.Run(() => worker.Remove());
        }

        private class AnnotationTitleWorker : WorkerBase<AnnotationTitleWorker>
        {
            // Provided in constructor arguments
            private readonly string inputPath;
            private CancellationToken token;


            internal AnnotationTitleWorker(string inputPath, CancellationToken token, 
                ILogbook logbook)
                : base(logbook)
            {
                this.inputPath = inputPath;
                this.token = token;
            }

            internal IEnumerable<string> GetTitles()
            {
                HashSet<string> results = new HashSet<string>();

                PdfDocument doc = new PdfDocument(new PdfReader(inputPath));
                int pageCount = doc.GetNumberOfPages();

                for (int i = 1; i <= pageCount; i++)
                {
                    if (token.IsCancellationRequested)
                        return null;

                    PdfPage page = doc.GetPage(i);
                    foreach (PdfAnnotation annot in page.GetAnnotations())
                    {
                        string title = annot?.GetTitle()?.GetValue();
                        if (title != null)
                            results.Add(title);
                    }
                }

                return results;
            }
        }

        private abstract class InternalExternalBase<TDerived> : WorkerBase<TDerived>
        {
            protected readonly CancellationToken token;

            protected PdfDocument doc;
            protected bool docIsInternal;

            internal InternalExternalBase(string inputPath, CancellationToken token, ILogbook logbook) 
                : this(new PdfDocument(new PdfWriter(inputPath)), token, logbook)
            { 
                this.token = token;
                docIsInternal = true;
            }

            internal InternalExternalBase(PdfDocument doc, CancellationToken token, ILogbook logbook)
                : base(logbook)
            {
                this.doc = doc;
                this.token = token;
            }

        }

        private class AnnotationRemovalWorker : InternalExternalBase<AnnotationRemovalWorker>
        {
            private readonly IEnumerable<string> annotationTitles;

            internal AnnotationRemovalWorker(IEnumerable<string> annotationTitles,
                string inputPath, CancellationToken token, ILogbook logbook) : 
                base(inputPath, token, logbook)
            { 
                this.annotationTitles = annotationTitles;
            }

            internal AnnotationRemovalWorker(IEnumerable<string> annotationTitles,
                PdfDocument doc, CancellationToken token, ILogbook logbook) 
                : base(doc, token, logbook)
            {
                this.annotationTitles = annotationTitles;
            }

            internal void Remove()
            {
                if (doc == null)
                    return;

                int pageCount = doc.GetNumberOfPages();

                for (int i = 1; i <= pageCount; i++)
                {
                    PdfPage page = doc.GetPage(i);
                    foreach (PdfAnnotation annot in page.GetAnnotations())
                    {
                        if (token.IsCancellationRequested)
                        {
                            if (docIsInternal)
                                doc.Close();
                            return;
                        }

                        string title = annot?.GetTitle()?.GetValue();
                        if (annotationTitles == null)
                            page.RemoveAnnotation(annot);
                        else if (title != null &&
                            annotationTitles.Contains(title))
                        {
                            page.RemoveAnnotation(annot);
                        }
                    }
                }

                if (docIsInternal)
                    doc.Close();
            }
        }

        private class RedactionFlatteningWorker : WorkerBase<RedactionFlatteningWorker>
        {
            private readonly CancellationToken token;
            private readonly string inputPath;

            internal RedactionFlatteningWorker(string inputPath, CancellationToken token, 
                ILogbook logbook) : base(logbook) 
            {
                this.token = token;
                this.inputPath = inputPath;
            }

            internal void Flatten()
            {
                string tempFile = IOPath.Combine(IOPath.GetDirectoryName(inputPath), IOPath.GetTempFileName());
                PdfDocument doc = new PdfDocument(new PdfReader(inputPath), new PdfWriter(tempFile));
                OpenedDocuments.Add(doc);
                CreatedPaths.Add(new FileInfo(tempFile));

                int pageCount = doc.GetNumberOfPages();

                for (int i = 1; i <= pageCount; i++)
                {
                    if (CheckIfCancelledAndCleanUp(token) == true)
                        return;

                    PdfPage page = doc.GetPage(i);

                    foreach (PdfAnnotation annotation in page.GetAnnotations())
                    {
                        PdfName annotName = annotation.GetSubtype();
                        if (annotName == PdfName.Redact || annotName == PdfName.Redaction)
                        {
                            PdfArray redactRectangle = annotation.GetRectangle();
                            PdfCanvas canvas = new PdfCanvas(page);
                            canvas.Rectangle(redactRectangle.ToRectangle());
                            canvas.SetStrokeColorRgb(255, 0, 0);
                            canvas.Stroke();

                            page.RemoveAnnotation(annotation);
                        }
                    }
                }

                doc.Close();

                File.Copy(tempFile, inputPath, true);
                File.Delete(tempFile);
            }
        }
    }
}
