using WF.LoggingLib;
using WF.PdfLib.Services;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Canvas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOPath = System.IO.Path;
using WF.PdfLib.Common.Redaction;
using iText.PdfCleanup;
using iText.PdfCleanup.Autosweep;
using iText.Kernel.Colors;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;

namespace WF.PdfLib.iText7
{
    /// <summary>
    /// Default implementation for <see cref="IAnnotationService"/>.
    /// </summary>
    public class AnnotationService : LoggingEnabled, IAnnotationService
    {
        /// <summary>
        /// Create a new annotation service implementation with logging.
        /// </summary>
        /// <param name="logbook">Logging service to use.</param>
        public AnnotationService(ILogbook logbook) : base(logbook) 
        { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="inputPath"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetTitles(string inputPath)
        {
            return await GetTitles(inputPath, CancellationToken.None);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetTitles(string inputPath,
            CancellationToken token)
        {
            AnnotationTitleWorker worker = new AnnotationTitleWorker(inputPath,
                token, logbook);
            return await Task.Run(() => worker.GetTitles());
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="inputPath"></param>
        /// <returns></returns>
        public async Task RemoveAll(string inputPath)
        {
            await RemoveByTitle(null, inputPath);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task RemoveAll(string inputPath, CancellationToken token)
        {
            await RemoveByTitle(null, inputPath, token);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="titles"></param>
        /// <param name="inputPath"></param>
        /// <returns></returns>
        public async Task RemoveByTitle(IEnumerable<string> titles, string inputPath)
        {
            await RemoveByTitle(titles, inputPath, CancellationToken.None);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="titles"></param>
        /// <param name="inputPath"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task RemoveByTitle(IEnumerable<string> titles, string inputPath,
            CancellationToken token)
        {
            AnnotationRemovalWorker worker = new AnnotationRemovalWorker(titles, inputPath,
                token, logbook);
            await Task.Run(() => worker.Remove());
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="inputPath"></param>
        /// <returns></returns>
        public async Task FlattenRedactions(string inputPath)
        {
            await FlattenRedactions(inputPath, CancellationToken.None);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task FlattenRedactions(string inputPath, CancellationToken token)
        {
            RedactionFlatteningWorker worker = new RedactionFlatteningWorker(inputPath, token, logbook);
            await Task.Run(() => worker.Flatten());
        }

        /// <summary>
        /// Internal execution method for the removal of all annotations.
        /// </summary>
        /// <param name="doc">Pdf document to remove annotations from.</param>
        /// <param name="token">Cancellation token for the current task.</param>
        /// <returns>An awaitable task.</returns>
        internal async Task RemoveAll(PdfDocument doc, CancellationToken token)
        {
            await RemoveByTitle(null, doc, token);
        }

        /// <summary>
        /// Internal execution method for the removal of specific annotations.
        /// </summary>
        /// <param name="titles">Titles of the annotations to remove.</param>
        /// <param name="doc">Pdf document to remove annotations from.</param>
        /// <param name="token">Cancellation token of the current task.</param>
        /// <returns>An awaitable task.</returns>
        internal async Task RemoveByTitle(IEnumerable<string> titles, PdfDocument doc,
            CancellationToken token)
        {
            AnnotationRemovalWorker worker = new AnnotationRemovalWorker(titles, doc,
                token, logbook);
            await Task.Run(() => worker.Remove());
        }

        public async Task CreateRedactions(string inputPath, string outputPath, CancellationToken token, List<IRedactionOption> options)
        {
            if (options == null || options.Count == 0) 
            {
                throw new ArgumentNullException(nameof(options));
            }

            RedactionCreationWorker worker = new RedactionCreationWorker(inputPath, outputPath, token, logbook);
            await Task.Run(() => worker.Redact(options));
        }

        public async Task ApplyRedactions(string inputPath, string outputPath, CancellationToken token)
        {
            RedactionCreationWorker worker = new RedactionCreationWorker(inputPath, outputPath, token, logbook);
            await Task.Run(() => worker.ApplyOnly());
        }

        /// <summary>
        /// Worker class for performing task with annotation titles.
        /// </summary>
        private class AnnotationTitleWorker : WorkerBase<AnnotationTitleWorker>
        {
            // Provided in constructor arguments
            private readonly string inputPath;
            private CancellationToken token;

            /// <summary>
            /// Create a new worker for working with annotation titles.
            /// </summary>
            /// <param name="inputPath">Path of the file to work with.</param>
            /// <param name="token">Cancellation token of the current task.</param>
            /// <param name="logbook">Logging service.</param>
            internal AnnotationTitleWorker(string inputPath, CancellationToken token, 
                ILogbook logbook)
                : base(logbook)
            {
                this.inputPath = inputPath;
                this.token = token;
            }

            /// <summary>
            /// Get the titles of annotations.
            /// </summary>
            /// <returns>Annotation titles.</returns>
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

        private class RedactionCreationWorker : WorkerBase<RedactionCreationWorker>
        {
            // Provided in constructor arguments
            private readonly string inputPath;
            private readonly string outputPath;
            private CancellationToken token;

            internal RedactionCreationWorker(
                string inputPath,
                string outputPath,
                CancellationToken token,
                ILogbook logbook) : base(logbook)
            {
                this.inputPath = inputPath;
                this.outputPath = outputPath;
                this.token = token;
            }

            internal void Redact(List<IRedactionOption> options)
            {
                string tempFile = GetTempFile();
                PdfDocument doc = GetPdfDoc(tempFile);

                foreach (IRedactionOption option in options.Where(x => x.Apply))
                {
                    if (token.IsCancellationRequested)
                    {
                        doc.Close();
                        return;
                    }

                    RedactAndApply(doc, option);
                }

                foreach(IRedactionOption option in options.Where(x => x.Apply == false))
                {
                    if (token.IsCancellationRequested)
                    {
                        doc.Close();
                        return;
                    }

                    MarkRedactsOnly(doc, option);
                }

                doc.Close();

                File.Copy(tempFile, outputPath, true);
                File.Delete(tempFile);
            }

            internal void ApplyOnly()
            {
                string tempFile = GetTempFile();
                PdfDocument doc = GetPdfDoc(tempFile);

                PdfCleaner.CleanUpRedactAnnotations(doc);

                doc.Close();

                File.Copy(tempFile, outputPath, true);
                File.Delete(tempFile);
            }

            private string GetTempFile()
            {
                return IOPath.Combine(IOPath.GetDirectoryName(inputPath), IOPath.GetTempFileName());
            }

            private PdfDocument GetPdfDoc(string tempFile)
            {
                PdfDocument doc = new PdfDocument(new PdfReader(inputPath), new PdfWriter(tempFile));
                OpenedDocuments.Add(doc);
                CreatedPaths.Add(new FileInfo(tempFile));
                
                return doc;
            }

            private void MarkRedactsOnly(PdfDocument doc, IRedactionOption option)
            {
                int pageCount = doc.GetNumberOfPages();

                for (int i = 1; i <= pageCount; i++)
                {
                    RegexBasedLocationExtractionStrategy strategy =
                    new RegexBasedLocationExtractionStrategy(option.GetRegex());

                    PdfCanvasProcessor parser = new PdfCanvasProcessor(strategy);

                    parser.ProcessPageContent(doc.GetPage(i));

                    foreach (IPdfTextLocation loc in strategy.GetResultantLocations().ToList())
                    {
                        PdfAnnotation redact = new PdfRedactAnnotation(loc.GetRectangle())
                            .SetTitle(new PdfString("Opus"))
                            .Put(PdfName.Subj, PdfName.Redact)
                            .Put(PdfName.IC, new PdfArray(option.Fill.GetFloat()))
                            .Put(PdfName.OC, new PdfArray(option.Outline.GetFloat()));
                        doc.GetPage(i).AddAnnotation(redact);
                    }
                }
            }

            private void RedactAndApply(PdfDocument doc, IRedactionOption option)
            {
                ICleanupStrategy strategy = new RegexBasedCleanupStrategy(option.GetRegex())
                    .SetRedactionColor(new DeviceRgb(option.Fill.GetColor()));
                PdfCleaner.AutoSweepCleanUp(doc, strategy);
            }
        }

        /// <summary>
        /// A helper class for determining, whether the pdf document instance used is produced here or elsewhere.
        /// </summary>
        /// <typeparam name="TDerived">Type of the class to implement this.</typeparam>
        private abstract class InternalExternalBase<TDerived> : WorkerBase<TDerived>
        {
            protected readonly CancellationToken token;

            /// <summary>
            /// The pdf document to evaluate.
            /// </summary>
            protected PdfDocument doc;

            /// <summary>
            /// If true, pdf document instance is created here.
            /// </summary>
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

        /// <summary>
        /// A worker class for dealing with annotation removals.
        /// </summary>
        private class AnnotationRemovalWorker : InternalExternalBase<AnnotationRemovalWorker>
        {
            private readonly IEnumerable<string> annotationTitles;

            /// <summary>
            /// Create a new worker for removing annotations from a pdf documents.
            /// </summary>
            /// <param name="annotationTitles">Annotations with any of these titles will be removed.</param>
            /// <param name="inputPath">File to remove annotations from.</param>
            /// <param name="token">Cancellation token of the current task.</param>
            /// <param name="logbook">Logging service.</param>
            internal AnnotationRemovalWorker(IEnumerable<string> annotationTitles,
                string inputPath, CancellationToken token, ILogbook logbook) : 
                base(inputPath, token, logbook)
            { 
                this.annotationTitles = annotationTitles;
            }

            /// <summary>
            /// Create a new worker for removing annotations from a pdf documents.
            /// </summary>
            /// <param name="annotationTitles">Annotations with any of these titles will be removed.</param>
            /// <param name="doc">Document to remove annotations from.</param>
            /// <param name="token">Cancellation token of the current task.</param>
            /// <param name="logbook">Logging service.</param>
            internal AnnotationRemovalWorker(IEnumerable<string> annotationTitles,
                PdfDocument doc, CancellationToken token, ILogbook logbook) 
                : base(doc, token, logbook)
            {
                this.annotationTitles = annotationTitles;
            }

            /// <summary>
            /// Exection method for removal.
            /// </summary>
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

        /// <summary>
        /// A worker class for flattening redaction annotations in a document.
        /// </summary>
        private class RedactionFlatteningWorker : WorkerBase<RedactionFlatteningWorker>
        {
            private readonly CancellationToken token;
            private readonly string inputPath;

            /// <summary>
            /// Create a new worker for flattening redaction annotations.
            /// </summary>
            /// <param name="inputPath">File to flatten.</param>
            /// <param name="token">Cancellation token of the current task.</param>
            /// <param name="logbook">Logging service.</param>
            internal RedactionFlatteningWorker(string inputPath, CancellationToken token, 
                ILogbook logbook) : base(logbook) 
            {
                this.token = token;
                this.inputPath = inputPath;
            }

            /// <summary>
            /// Internal execution method for flattening the redaction annotations in a document.
            /// </summary>
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
