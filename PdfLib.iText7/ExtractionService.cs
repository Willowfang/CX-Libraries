using System;
using System.Collections.Generic;
using System.Linq;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using CX.PdfLib.Extensions;
using System.IO;
using CX.PdfLib.Common;
using System.Threading;
using System.Threading.Tasks;
using CX.LoggingLib;

namespace CX.PdfLib.iText7
{
    public class ExtractionService : LoggingEnabled<ExtractionService>, IExtractionService
    {
        private IPdfAConvertService pdfAConverter;

        public ExtractionService(IPdfAConvertService pdfAConverter, ILogbook logbook) : base(logbook)
        {
            this.pdfAConverter = pdfAConverter;
        }

        /// <summary>
        /// Extract a pdf file according to <see cref="ExtractionOptions"/>.
        /// </summary>
        /// <param name="options"></param>
        /// <returns>A list of created files and folders (of which all might not exist)</returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<IList<FileSystemInfo>> Extract(ExtractionOptions options)
        {
            ExtractionWorker worker;

            try
            {
                worker = new ExtractionWorker(options, pdfAConverter, logbook.BaseLogbook);
            }
            catch (ArgumentException e)
            {
                logbook.Write($"{nameof(ExtractionWorker)} creation failed.", LogLevel.Error, e);
                throw;
            }

            return await worker.Extract();
        }
    }

    internal class ExtractionWorker : WorkerBase<ExtractionWorker>
    {
        private ExtractionOptions options;
        private PdfMerger merger;
        private DirectoryInfo destinationDirectory;
        private DirectoryInfo workingDirectory;
        private PdfDocument destinationDocument;
        private string destinationDocumentPath;
        private int totalCount;
        private int currentCount;
        private IPdfAConvertService pdfAConverter;
        private Utilities utilities;

        private bool groupByFiles;

        internal ExtractionWorker(ExtractionOptions options, IPdfAConvertService pdfAConverter,
            ILogbook logbook) : base(logbook)
        {
            this.options = options;
            this.pdfAConverter = pdfAConverter;
            utilities = new Utilities(logbook);

            PrepareCleanUp();
            CreateDestination();
            GetTotalCount();
            currentCount = 0;

            groupByFiles = options.Files.Any(e => e.Extractables.Count() > 1);
        }

        private void CreateDestination()
        {
            if (options.PdfA)
            {
                string path;

                if (options.Destination is DirectoryInfo dir)
                    path = Path.Combine(dir.FullName, Path.GetRandomFileName());
                else if (options.Destination is FileInfo file)
                {
                    path = Path.Combine(Path.GetDirectoryName(file.FullName), Path.GetRandomFileName());
                }
                else
                    throw new ArgumentException(nameof(options.Destination));

                workingDirectory = new DirectoryInfo(path);
            }
            else
            {
                if (options.Destination is DirectoryInfo dir)
                    workingDirectory = dir;
                else if (options.Destination is FileInfo file)
                    workingDirectory = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(file.FullName),
                        Path.GetRandomFileName()));
                else
                    throw new ArgumentException(nameof(options.Destination));
            }

            if (workingDirectory.Exists == false)
            {
                CreatedPaths.Add(workingDirectory);
                workingDirectory.Create();
            }

            if (options.Destination is FileInfo destFile)
            {
                string workPath = Path.Combine(workingDirectory.FullName, destFile.Name);
                destinationDocument = new PdfDocument(new PdfWriter(workPath));
                destinationDocumentPath = workPath;
                CreatedPaths.Add(new FileInfo(workPath));

                merger = new PdfMerger(destinationDocument, false, true);

                destinationDirectory = new DirectoryInfo(Path.GetDirectoryName(destFile.FullName));
            }
            else
            {
                destinationDirectory = (DirectoryInfo)options.Destination;
            }
        }
        private void GetTotalCount()
        {
            foreach (FileAndExtractables file in options.Files)
            {
                totalCount += file.Extractables.Count();
            }

            if (options.PdfA)
            {
                if (options.Destination is DirectoryInfo)
                    totalCount *= 2;
                else if (options.Destination is FileInfo)
                    totalCount += 1;
                else
                    throw new ArgumentException("Destination not valid.");
            }
        }

        public async Task<IList<FileSystemInfo>> Extract()
        {
            PrepareCleanUp();

            // Loop through each pair of files and bookmarks to extract
            foreach (FileAndExtractables file in options.Files)
            {
                await OperateOnFile(file);
                if (CheckIfCancelledAndCleanUp(options.Cancellation)) return CreatedPaths;
            }

            if (destinationDirectory.FullName != workingDirectory.FullName)
            {
                CreatedPaths.Add(workingDirectory);

                if (options.Destination is FileInfo product)
                {
                    logbook.Write($"Single file requested. Merger starting.", LogLevel.Debug);
                    if (options.Progress != null)
                        options.Progress.Report(new ProgressReport(currentCount * 100 / totalCount, ProgressPhase.Merging));

                    FileInfo[] files = workingDirectory.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        if (file.FullName == destinationDocumentPath)
                            continue;
                        PdfDocument tempDoc = new PdfDocument(new PdfReader(file.FullName));
                        merger.Merge(tempDoc, 1, tempDoc.GetNumberOfPages());
                        tempDoc.Close();
                        file.Delete();
                    }

                    destinationDocument.Close();

                    logbook.Write($"Merging complete.", LogLevel.Debug);
                }

                if (options.PdfA)
                {
                    logbook.Write($"Pdf/a requested. Starting conversion.", LogLevel.Debug);

                    if (options.Progress != null)
                        options.Progress.Report(new ProgressReport(0, ProgressPhase.Converting));

                    foreach (FileInfo file in workingDirectory.GetFiles())
                    {
                        FileInfo creationFile = new FileInfo(Path.Combine(destinationDirectory.FullName, file.Name));
                        if (creationFile.Exists == false)
                            CreatedPaths.Add(creationFile);
                    }

                    bool noError = await pdfAConverter.Convert(workingDirectory, destinationDirectory, options.Cancellation);

                    if (CheckIfCancelledAndCleanUp(options.Cancellation)) return CreatedPaths;

                    PdfAConversionEventArgs args = new PdfAConversionEventArgs();
                    args.WasFaulted = !noError;
                    args.Path = destinationDirectory.FullName;
                    options.OnPdfAConversionFinished(args);

                    logbook.Write($"Pdf/a conversion complete.", LogLevel.Debug);
                }
                else
                {
                    foreach (FileInfo file in workingDirectory.GetFiles())
                    {
                        file.MoveTo(Path.Combine(destinationDirectory.FullName, file.Name), true);
                    }
                }

                workingDirectory.Delete(true);
            }

            return CreatedPaths;
        }

        private async Task OperateOnFile(FileAndExtractables file)
        {
            var doc = new PdfDocument(new PdfReader(file.FilePath));
            OpenedDocuments.Add(doc);

            IEnumerable<ILeveledBookmark> originalDocumentBookmarks = utilities.FindLeveledBookmarks(doc, false);

            if (options.Destination is DirectoryInfo)
            {
                foreach (ILeveledBookmark bookmark in file.Extractables)
                {
                    // Only show progress, if the amount of bookmarks to extract is significant or they will be converted
                    if ((options.Progress != null && totalCount > 50) || options.PdfA)
                    {
                        options.Progress.Report(new ProgressReport(currentCount * 100 / totalCount, ProgressPhase.Extracting,
                            bookmark.Title));
                    }

                    await ExtractAsSeparate(bookmark, doc, file, originalDocumentBookmarks);
                    if (CheckIfCancelledAndCleanUp(options.Cancellation)) return;
                }
            }
            else
            {
                IList<int> allPages = GetAllPages(file.Extractables);

                string fileName = groupByFiles == true ? Path.GetFileName(file.FilePath) : file.Extractables.First().Title + ".pdf";

                FileInfo destinationFile = new FileInfo(Path.Combine(workingDirectory.FullName,
                    fileName));

                PdfDocument result = await ExtractPages(destinationFile, allPages, doc);

                OpenedDocuments.Add(result);
                CreatedPaths.Add(destinationFile);

                CopyOutlinesForSingleFile(doc, result, file.Extractables, allPages,
                    Path.GetFileNameWithoutExtension(fileName));

                result.Close();
            }

            doc.Close();
        }

        private async Task ExtractAsSeparate(ILeveledBookmark bookmark, PdfDocument doc, FileAndExtractables file,
            IEnumerable<ILeveledBookmark> originalDocumentBookmarks)
        {
            FileInfo destinationFile = new FileInfo(Path.Combine(workingDirectory.FullName,
                    bookmark.Title.ReplaceIllegal() + ".pdf"));

            PdfDocument result = await ExtractPages(destinationFile, bookmark.Pages, doc);

            CopyOutlinesForMultipleFile(result, bookmark, originalDocumentBookmarks);

            result.Close();
            currentCount++;
        }

        private void CopyOutlinesForMultipleFile(PdfDocument doc, ILeveledBookmark bookmark,
            IEnumerable<ILeveledBookmark> originalDocumentBookmarks)
        {
            // Remove bookmarks copied with merging
            doc.GetCatalog().Remove(PdfName.Outlines);

            // Find children of the current bookmark in the original list and adjust their start pages
            // to match the new document
            IList<ILeveledBookmark> children = utilities.AdjustBookmarksExtract(originalDocumentBookmarks.ToList(),
                bookmark.Pages);

            // If there are children, adjust their levels to match the new document tree
            if (children.Count > 1)
            {
                // Remove the bookmark being extracted from previous list, if it has no exclusive pages
                if (children[0].StartPage == children[1].StartPage && options.Destination is DirectoryInfo)
                {
                    children.RemoveAt(0);
                }

                IList<ILeveledBookmark> levelAdjustedChildren = children.AdjustLevels(1 - children[0].Level);
                utilities.AddLeveledBookmarks(levelAdjustedChildren, doc);
            }
            else
            {
                if (children.Count == 1 && options.Destination is DirectoryInfo)
                    children.RemoveAt(0);
            }
        }

        private void CopyOutlinesForSingleFile(PdfDocument source, PdfDocument result, 
            IEnumerable<ILeveledBookmark> extractables, IList<int> allUniquePagesSorted, string fileNameTitle)
        {
            result.GetCatalog().Remove(PdfName.Outlines);

            IList<ILeveledBookmark> products = new List<ILeveledBookmark>();
            IList<ILeveledBookmark> docBookmarks = utilities.FindLeveledBookmarks(source);

            foreach (ILeveledBookmark original in extractables)
            {
                IList<ILeveledBookmark> parentAndChildren = utilities.GetParentAndChildrenForExtraction(docBookmarks, original);
                foreach (ILeveledBookmark child in parentAndChildren)
                {
                    if (products.Contains(child) == false) products.Add(child);
                }
            }

            products = utilities.AdjustBookmarksExtract(products, allUniquePagesSorted);

            products = products.AdjustLevels(1);

            if (groupByFiles)
            {
                products.Insert(0, new LeveledBookmark(1, fileNameTitle, 1, 1));
            }

            utilities.AddLeveledBookmarks(products, result);
        }

        private IList<int> GetAllPages(IEnumerable<ILeveledBookmark> bookmarks)
        {
            HashSet<int> uniquePages = new HashSet<int>();
            foreach (ILeveledBookmark mark in bookmarks)
            {
                // Only show progress, if the amount of bookmarks to extract is significant or they will be converted
                if ((options.Progress != null && totalCount > 50) || options.PdfA)
                {
                    options.Progress.Report(new ProgressReport(currentCount * 100 / totalCount, ProgressPhase.Extracting,
                        mark.Title));
                }

                foreach (int page in mark.Pages)
                    uniquePages.Add(page);

                currentCount++;
            }
            List<int> allPages = uniquePages.ToList();

            allPages.Sort();
            return allPages;
        }

        private async Task<PdfDocument> ExtractPages(FileInfo destinationFile, IList<int> pages, PdfDocument source)
        {
            ExtSplitter split = new ExtSplitter(source, pageRange => new PdfWriter(destinationFile));
            PdfDocument result = split.ExtractPages(pages);

            OpenedDocuments.Add(result);
            CreatedPaths.Add(destinationFile);

            if (options.Annotations != AnnotationOption.Keep)
            {
                AnnotationService annotations = new AnnotationService(logbook.BaseLogbook);
                if (options.Annotations == AnnotationOption.RemoveUser)
                {
                    await annotations.RemoveByTitle(options.AnnotationUsersToRemove, result, options.Cancellation);
                }
                else if (options.Annotations == AnnotationOption.RemoveAll)
                {
                    await annotations.RemoveAll(result, options.Cancellation);
                }
            }

            utilities.Flatten(result);

            return result;
        }

        internal class ExtSplitter : PdfSplitter
        {
            private Func<PageRange, PdfWriter> nextWriter;
            internal ExtSplitter(PdfDocument doc, Func<PageRange, PdfWriter> nextWriter) : base(doc)
            {
                this.nextWriter = nextWriter;
            }

            protected override PdfWriter GetNextPdfWriter(PageRange documentPageRange)
            {
                return nextWriter.Invoke(documentPageRange);
            }

            public PdfDocument ExtractPages(IList<int> pages)
            {
                string range = RangesAsString(pages);
                return ExtractPageRange(new PageRange(range));
            }

            private string RangesAsString(IList<int> pages)
            {
                if (pages.Count() == 0) return null;

                // Create a sorted list of without duplicate pages
                // and begin the ranges string with the first page
                List<int> sortedPages = pages.Distinct().ToList();
                sortedPages.Sort();

                // All the pages individually or as ranges (e.g. 1-5)
                List<string> ranges = new List<string>();

                // Set the first string with first page number
                int begin = sortedPages[0];
                string range = begin.ToString();

                // Add all other pages to the string
                for (int i = 1; i <= sortedPages.Count; i++)
                {
                    // If page is not the last page and is the next consecutive page,
                    // create or extend a range within the string (e.g. 1-2 => 1-3).
                    if (i < sortedPages.Count && sortedPages[i] == sortedPages[i - 1] + 1)
                    {
                        range = $"{begin}-{sortedPages[i]}";
                        continue;
                    }

                    ranges.Add(range);

                    // Do not create a new range for the last page,
                    // otherwise reset the range
                    if (i < sortedPages.Count)
                    {
                        begin = sortedPages[i];
                        range = begin.ToString();
                    }
                }

                return string.Join(", ", ranges);
            }
        }
    }
}
