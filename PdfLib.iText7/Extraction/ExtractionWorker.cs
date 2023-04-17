using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Kernel.XMP.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WF.LoggingLib;
using WF.PdfLib.Common;
using WF.PdfLib.Extensions;
using WF.PdfLib.Services;
using WF.PdfLib.Services.Data;

namespace WF.PdfLib.iText7.Extraction
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class ExtractionWorker : WorkerBase<ExtractionWorker>, IExtractionWorker
    {
        private readonly IPdfAConvertService convertService;

        public event EventHandler<BookmarkOrFileExtractedEventArgs>? BookmarkOrFileExtracted;

        public event EventHandler<FileMergedEventArgs>? FileMerged;

        public event EventHandler<FilesConvertedToPdfAEventArgs>? FilesConverted;

        /// <summary>
        /// Create new implementation instance.
        /// </summary>
        /// <param name="convertService">Service for converting files to pdf/a.</param>
        /// <param name="logbook">Logging service.</param>
        public ExtractionWorker(
            IPdfAConvertService convertService,
            ILogbook logbook) : base(logbook)
        {
            this.convertService = convertService;
            PrepareCleanUp();
        }

        #region Extraction
        public async Task<DirectoryInfo> Extract(ExtractionOptions options)
        {
            PrepareCleanUp();

            DirectoryInfo workDirectory;

            try
            {
                workDirectory = GetWorkDirectory(options);
            }
            catch (Exception ex)
            {
                logbook.Write("Work directory retrieval failed.", LogLevel.Error, ex);
                throw;
            }

            // Loop through each pair of file and bookmarks to extract.
            foreach (FileAndExtractables file in options.Files)
            {
                await OperateOnFile(file, options, workDirectory);

                if (options.Destination is FileInfo) 
                    OnBookmarkOrFileExtracted(file.FilePath, options.Files.Last() == file);

                CheckIfCancelledAndCleanUp(options.Cancellation);
            }

            if (options.Destination is FileInfo destinationFile)
            {
                await MergeAndMoveProducts(workDirectory, options, destinationFile);
            }
            else if (options.ConvertToPdfA && options.Destination is DirectoryInfo destinationDirectory)
            {
                await ConvertToPdfA(workDirectory, destinationDirectory, convertService, options.Cancellation);
            }

            if (EvaluateWorkDirectoryDelete(workDirectory, options))
            {
                workDirectory.Delete(true);
            }

            return workDirectory;
        }

        private bool EvaluateWorkDirectoryDelete(DirectoryInfo workDirectory, ExtractionOptions options)
        {
            if (options.Destination is FileInfo)
            {
                return true;
            }
            else if (options.Destination is DirectoryInfo directory &&
                directory.FullName == workDirectory.FullName)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private async Task MergeAndMoveProducts(
            DirectoryInfo workDirectory,
            ExtractionOptions options,
            FileInfo destinationFile)
        {
            FileInfo workFile = destinationFile;

            if (options.ConvertToPdfA)
            {
                workFile = new FileInfo(Path.Combine(workDirectory.FullName, destinationFile.Name));
            }

            await MergeAndDelete(workDirectory, workFile);

            if (options.ConvertToPdfA)
            {
                await ConvertToPdfA(workDirectory, destinationFile.Directory!, convertService, options.Cancellation);
            }
        }

        private DirectoryInfo GetWorkDirectory(ExtractionOptions options)
        {
            if (options.Destination is DirectoryInfo directory)
            {
                if (options.ConvertToPdfA)
                {
                    return CreateTemporaryWorkingDirectory(directory);
                }
                else
                {
                    return directory;
                }
            }
            else if (options.Destination is FileInfo file)
            {
                if (file.Directory == null)
                {
                    throw new ArgumentNullException("Extraction to file does not have parent directory");
                }

                return CreateTemporaryWorkingDirectory(file.Directory);
            }
            else
            {
                throw new ArgumentException("Extraction destination is not a file or a directory.");
            }
        }

        private async Task OperateOnFile(
            FileAndExtractables file, 
            ExtractionOptions options,
            DirectoryInfo workDirectory)
        {
            var doc = new PdfDocument(new PdfReader(file.FilePath));

            OpenedDocuments.Add(doc);

            if (options.Destination is DirectoryInfo)
            {
                foreach (ILeveledBookmark bookmark in file.Extractables)
                {
                    await ExtractAsSeparate(bookmark, doc, options, workDirectory);

                    OnBookmarkOrFileExtracted(bookmark.Title, file.Extractables.Last() == bookmark);

                    if (CheckIfCancelledAndCleanUp(options.Cancellation)) return;
                }
            }
            else
            {
                await OperateOnSingleFile(file, options, doc, workDirectory);
            }

            doc.Close();
        }

        private async Task OperateOnSingleFile(
            FileAndExtractables file, 
            ExtractionOptions options, 
            PdfDocument doc,
            DirectoryInfo workDirectory)
        {
            bool groupByFiles = options.Files.Any(e => e.Extractables.Count() > 1);

            IList<int> allPages = GetAllPages(file.Extractables);

            string fileName = groupByFiles == true ? Path.GetFileName(file.FilePath) : file.Extractables.First().Title + ".pdf";

            FileInfo destinationFile = new FileInfo(Path.Combine(workDirectory.FullName,
                fileName.ReplaceIllegal()));

            PdfDocument result = await ExtractPages(destinationFile, allPages, doc, options);

            OpenedDocuments.Add(result);
            CreatedPaths.Add(destinationFile);

            /*CopyOutlinesForSingleFile(
                doc, 
                result, 
                file.Extractables, 
                allPages,
                Path.GetFileNameWithoutExtension(fileName),
                groupByFiles);*/

            result.Close();
        }

        private async Task ExtractAsSeparate(
            ILeveledBookmark bookmark, 
            PdfDocument doc, 
            ExtractionOptions options,
            DirectoryInfo workDirectory)
        {
            FileInfo destinationFile = new FileInfo(Path.Combine(workDirectory.FullName,
                    bookmark.Title.ReplaceIllegal() + ".pdf"));

            PdfDocument result = await ExtractPages(destinationFile, bookmark.Pages, doc, options);

            CopyOutlinesForMultipleFile(result, bookmark, options);

            result.Close();
        }

        private void CopyOutlinesForMultipleFile(
            PdfDocument doc, 
            ILeveledBookmark bookmark, 
            ExtractionOptions options)
        {
            Utilities utilities = new Utilities(logbook.BaseLogbook);

            IEnumerable<ILeveledBookmark>? originalDocumentBookmarks = utilities.FindLeveledBookmarks(doc, false);

            if (originalDocumentBookmarks == null || originalDocumentBookmarks.Count() < 1) return;

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

        private void CopyOutlinesForSingleFile(
            PdfDocument source,
            PdfDocument result,
            IEnumerable<ILeveledBookmark> extractables,
            IList<int> allUniquePagesSorted,
            string fileNameTitle,
            bool groupByFiles)
        {
            Utilities utilities = new Utilities(logbook.BaseLogbook);

            result.GetCatalog().Remove(PdfName.Outlines);

            IList<ILeveledBookmark> products = new List<ILeveledBookmark>();
            IList<ILeveledBookmark>? docBookmarks = utilities.FindLeveledBookmarks(source);

            foreach (ILeveledBookmark original in extractables)
            {
                if (docBookmarks != null)
                {
                    IList<ILeveledBookmark> parentAndChildren = utilities.GetParentAndChildrenForExtraction(docBookmarks, original);
                    foreach (ILeveledBookmark child in parentAndChildren)
                    {
                        if (products.Contains(child) == false) products.Add(child);
                    }
                }
                else
                {
                    products.Add(original);
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
                foreach (int page in mark.Pages)
                    uniquePages.Add(page);
            }
            List<int> allPages = uniquePages.ToList();

            allPages.Sort();
            return allPages;
        }

        private async Task<PdfDocument> ExtractPages(
            FileInfo destinationFile, 
            IList<int> pages, 
            PdfDocument source,
            ExtractionOptions options)
        {
            Utilities utilities = new Utilities(logbook.BaseLogbook);

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
                string? range = RangesAsString(pages);
                return ExtractPageRange(new PageRange(range));
            }

            private string? RangesAsString(IList<int> pages)
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

        #endregion

        #region Merge
        private async Task<FileInfo> MergeAndDelete(DirectoryInfo source, FileInfo destination)
        {
            logbook.Write($"Starting merge.", LogLevel.Debug);

            FileInfo[] files = source.GetFiles();

            PdfDocument destinationDocument = new PdfDocument(new PdfWriter(destination.FullName));

            PdfMerger merger = new PdfMerger(destinationDocument, false, true);

            foreach (FileInfo file in files)
            {
                if (file.FullName == destination.FullName)
                    continue;

                PdfDocument tempDoc = new PdfDocument(new PdfReader(file.FullName));

                await Task.Run(() => merger.Merge(tempDoc, 1, tempDoc.GetNumberOfPages()));

                tempDoc.Close();

                file.Delete();

                OnFileMerged(file.FullName);
            }

            destinationDocument.Close();

            logbook.Write($"Merging complete.", LogLevel.Debug);

            return destination;
        }
        #endregion

        #region Convert
        private async Task<IList<FileSystemInfo>?> ConvertToPdfA(
            DirectoryInfo source, 
            DirectoryInfo destination,
            IPdfAConvertService pdfAConverter,
            CancellationToken cancellation = default(CancellationToken))
        {
            logbook.Write($"Starting pdf/a conversion.", LogLevel.Debug);

            FileInfo[] sourceFiles = source.GetFiles();

            foreach (FileInfo file in sourceFiles)
            {
                FileInfo creationFile = new FileInfo(Path.Combine(destination.FullName, file.Name));

                if (creationFile.Exists == false)
                    CreatedPaths.Add(creationFile);
            }

            bool noError = await pdfAConverter.Convert(source, destination, cancellation);

            if (CheckIfCancelledAndCleanUp(cancellation)) return CreatedPaths;

            if (noError == false)
            {
                foreach (FileInfo file in sourceFiles)
                {
                    file.CopyTo(Path.Combine(destination.FullName, file.Name), true);
                }
            }

            OnFilesConverted(!noError, destination.FullName);

            foreach (FileInfo file in sourceFiles)
            {
                file.Delete();
            }

            logbook.Write($"Pdf/a conversion complete.", LogLevel.Debug);

            return null;
        }
        #endregion

        #region Working directory
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        private DirectoryInfo CreateTemporaryWorkingDirectory(DirectoryInfo rootDirectory)
        {
            DirectoryInfo temp = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));

            if (temp.Exists == false) temp.Create();

            return temp;
        }
        #endregion

        #region Event callers
        protected virtual void OnBookmarkOrFileExtracted(string name, bool isDone = false)
        {
            BookmarkOrFileExtractedEventArgs e = new BookmarkOrFileExtractedEventArgs(name, isDone);
            BookmarkOrFileExtracted?.Invoke(this, e);
        }

        protected virtual void OnFileMerged(string filePath, bool isDone = false)
        {
            FileMergedEventArgs e = new FileMergedEventArgs(filePath, isDone);
            FileMerged?.Invoke(this, e);
        }

        protected virtual void OnFilesConverted(bool faulted, string destinationDirectoryPath)
        {
            FilesConvertedToPdfAEventArgs e = new FilesConvertedToPdfAEventArgs(faulted, destinationDirectoryPath);
            FilesConverted?.Invoke(this, e);
        }
        #endregion
    }
}
