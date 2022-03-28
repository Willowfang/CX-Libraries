using CX.LoggingLib;
using CX.PdfLib.Common;
using CX.PdfLib.Extensions;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CX.PdfLib.iText7
{
    public class MergingService : LoggingEnabled, IMergingService
    {
        private IWordConvertService wordConvertService;

        public MergingService(IWordConvertService wordConvertService,
            ILogbook logbook) : base(logbook) 
        {
            this.wordConvertService = wordConvertService;
        }

        public async Task<IList<int>> Merge(IList<string> sourcePaths, string outputPath)
        {
            return await Merge(sourcePaths, outputPath, CancellationToken.None);
        }

        public async Task<IList<int>> Merge(IList<string> sourcePaths, string outputPath, CancellationToken token)
        {
            MergingWorker worker = new MergingWorker(sourcePaths, outputPath, token, logbook);
            return await Task.Run(() => worker.Merge());
        }

        public async Task<IList<FileSystemInfo>> MergeWithOptions(MergingOptions options)
        {
            MergingOptionsWorker worker = new MergingOptionsWorker(options, wordConvertService,
                logbook);
            return await worker.Merge();
        }

        private class MergingWorker : OperatorBase<MergingWorker>
        {
            // Passed in constructor
            private readonly IList<string> sourcePaths;
            private readonly string outputPath;
            private readonly CancellationToken token;

            // Created in-class
            private PdfDocument product;
            private PdfMerger merger;
            private readonly List<int> startPages;
            private int currentStartPage;

            internal MergingWorker(IList<string> sourcePaths, string outputPath,
                CancellationToken token, ILogbook logbook) : base(logbook)
            {
                this.sourcePaths = sourcePaths;
                this.outputPath = outputPath;
                this.token = token;
                startPages = new List<int>();
                currentStartPage = 1;
            }

            internal IList<int> Merge()
            {
                try
                {
                    return ExecuteMerge();
                }
                catch (Exception)
                {
                    CleanUp();
                    throw;
                }
            }

            private IList<int> ExecuteMerge()
            {
                product = new PdfDocument(new PdfReader(outputPath));
                OpenedDocuments.Add(product);
                merger = new PdfMerger(product).SetCloseSourceDocuments(true);

                string dir = Path.GetDirectoryName(outputPath);
                if (Directory.Exists(dir) == false)
                {
                    Directory.CreateDirectory(dir);
                    CreatedPaths.Add(new DirectoryInfo(dir));
                }

                try
                {
                    foreach (string path in sourcePaths)
                    {
                        if (CheckIfFileExistsAndCleanUp(path))
                        {
                            throw new ArgumentException($"File at {path} does not exist.");
                        }
                        if (CheckIfCancelledAndCleanUp(token))
                        {
                            return startPages;
                        }

                        MergeDocument(path);
                    }
                }
                catch (Exception e)
                {
                    logbook.Write($"Merging failed at {nameof(MergingWorker)}.", LogLevel.Error, e);
                    CleanUp();
                    throw;
                }

                return startPages;
            }

            private void MergeDocument(string path)
            {
                startPages.Add(currentStartPage);
                if (path != null)
                {
                    PdfDocument src = new PdfDocument(new PdfReader(path));
                    OpenedDocuments.Add(src);
                    int srcPages = src.GetNumberOfPages();
                    merger.Merge(src, 1, srcPages);
                    currentStartPage += srcPages;
                }
            }
        }
        
        private class MergingOptionsWorker : OperatorBase<MergingOptionsWorker>
        {
            // Provided by constructor arguments
            private readonly IWordConvertService wordConvertService;
            private readonly MergingOptions options;

            // Created in-class
            private readonly Utilities utilities;
            private int totalStages;
            private PdfDocument destination;
            private IList<string> convertedFilesAsPaths;
            private IList<int> startPages;
            private int outputPageCount;
            private List<ILeveledBookmark> bookmarks;

            internal MergingOptionsWorker(MergingOptions options, IWordConvertService wordConvertService, 
                ILogbook logbook)
                : base(logbook)
            {
                this.options = options;
                this.wordConvertService = wordConvertService;
                utilities = new Utilities(logbook);
                totalStages = 5;
                startPages = new List<int>();
                bookmarks = new List<ILeveledBookmark>();
                if (options.AddPageNumbers)
                {
                    totalStages++;
                }
            }

            internal async Task<IList<FileSystemInfo>> Merge()
            {
                try
                {
                    return await ExecuteMerge();
                }
                catch (Exception)
                {
                    CleanUp();
                    throw;
                }
            }

            private async Task<IList<FileSystemInfo>> ExecuteMerge()
            {
                if (options.OutputFile.Exists == false)
                {
                    CreatedPaths.Add(options.OutputFile);
                }

                options.Progress?.Report(new ProgressReport(1 * 100 / totalStages, ProgressPhase.Converting));

                await ConvertWordFiles();

                if (CheckIfCancelledAndCleanUp(options.Cancellation)) return null;

                options.Progress?.Report(new ProgressReport(2 * 100 / totalStages, ProgressPhase.Merging));

                MergeFiles();

                destination = new PdfDocument(new PdfWriter(options.OutputFile));
                OpenedDocuments.Add(destination);

                if (CheckIfCancelledAndCleanUp(options.Cancellation)) return null;

                outputPageCount = destination.GetNumberOfPages();

                options.Progress?.Report(new ProgressReport(3 * 100 / totalStages, ProgressPhase.GettingBookmarks));

                RetrieveAndAdjustBookmarks();

                if (CheckIfCancelledAndCleanUp(options.Cancellation)) return null;

                options.Progress?.Report(new ProgressReport(4 * 100 / totalStages, ProgressPhase.AddingBookmarks));

                InsertBookmarks();

                if (CheckIfCancelledAndCleanUp(options.Cancellation)) return null;

                if (options.AddPageNumbers)
                {
                    AddPageNumbers();
                }

                if (destination.IsClosed() == false) destination.Close();

                options.Progress?.Report(new ProgressReport(100, ProgressPhase.Finished));

                return CreatedPaths;
            }

            private async Task ConvertWordFiles()
            {
                List<string> filePaths = options.Inputs.Select(x => x.FilePath).ToList();
                convertedFilesAsPaths = await wordConvertService.Convert(filePaths, null, options.Cancellation);

                if (CheckIfCancelledAndCleanUp(options.Cancellation)) return;

                foreach (string path in convertedFilesAsPaths.Except(filePaths))
                {
                    CreatedPaths.Add(new FileInfo(path));
                }
            }

            private void MergeFiles()
            {
                MergingWorker merger = new MergingWorker(convertedFilesAsPaths, options.OutputFile.FullName,
                    options.Cancellation, logbook.BaseLogbook);

                startPages = merger.Merge();
            }

            private void RetrieveAndAdjustBookmarks()
            {
                List<IMergeInput> inputList = options.Inputs.ToList();

                for (int i = 0; i < inputList.Count; i++)
                {
                    IMergeInput current = inputList[i];

                    bookmarks.Add(new LeveledBookmark(current.Level, current.Title,
                        startPages[i], 1));

                    if (current.FilePath != null && Path.GetExtension(current.FilePath).ToLower() == ".pdf")
                    {
                        IList<ILeveledBookmark> leveledOriginal = utilities.FindLeveledBookmarks(
                            new PdfDocument(new PdfReader(current.FilePath))).AdjustLevels(current.Level);

                        bookmarks.AddRange(utilities.AdjustBookmarksMerge(leveledOriginal, startPages[i]));
                    }
                }
            }

            private void InsertBookmarks()
            {
                destination.GetCatalog().Remove(PdfName.Outlines);
                utilities.AddLeveledBookmarks(utilities.GetAllPages(bookmarks, outputPageCount), destination);
            }

            private void AddPageNumbers()
            {
                options.Progress?.Report(new ProgressReport(5 * 100 / totalStages, ProgressPhase.AddingPageNumbers));

                Document doc = new Document(destination);

                try
                {
                    float positionModifier = 0.05f;
                    float fontSizeModifier = 0.02f;
                    iText.Layout.Properties.TextAlignment horizontalAlignment = iText.Layout.Properties.TextAlignment.RIGHT;
                    iText.Layout.Properties.VerticalAlignment verticalAlignment = iText.Layout.Properties.VerticalAlignment.TOP;

                    for (int i = 1; i <= doc.GetPdfDocument().GetNumberOfPages(); i++)
                    {
                        iText.Kernel.Geom.Rectangle rect = doc.GetPdfDocument().GetPage(i).GetPageSize();
                        float x = rect.GetRight() - (rect.GetWidth() * positionModifier);
                        float y = rect.GetTop() - (rect.GetHeight() * positionModifier);
                        Paragraph para = new Paragraph($"{i}");
                        para.SetFontSize(rect.GetHeight() * fontSizeModifier);
                        doc.ShowTextAligned(para, x, y, i, horizontalAlignment, verticalAlignment, 0);
                        // 520, 780
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    doc.Close();
                }
            }
        }
    }
}
