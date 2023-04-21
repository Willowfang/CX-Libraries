using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Navigation;
using iText.Kernel.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.XPath;
using WF.LoggingLib;
using WF.PdfLib.Common;
using WF.PdfLib.Extensions;
using WF.PdfLib.Services;
using WF.PdfLib.Services.Bookmarks;

namespace WF.PdfLib.iText7
{
    public class DocumentBookmarkHandlerFactory : IDocumentBookmarkHandlerFactory
    {
        private ILogbook logbook;

        public DocumentBookmarkHandlerFactory(ILogbook logbook)
        {
            this.logbook = logbook;
        }

        public IDocumentBookmarkHandler Create(string filePath)
        {
            return new DocumentBookmarkHandler(filePath, logbook);
        }

        public IDocumentBookmarkHandler Create(string filePath, IBookmarkFactory bookmarkFactory)
        {
            return new DocumentBookmarkHandler(filePath, bookmarkFactory, logbook);
        }
    }
    public class BookmarkHandlerFactory : IBookmarkHandlerFactory
    {
        private ILogbook logbook;
        private IPdfAConvertService pdfAConvertService;
        private IWordConvertService wordConvertService;

        public BookmarkHandlerFactory(
            ILogbook logbook, 
            IPdfAConvertService pdfAConvertService, 
            IWordConvertService wordConvertService)
        {
            this.logbook = logbook;
            this.pdfAConvertService = pdfAConvertService;
            this.wordConvertService = wordConvertService;
        }

        public IBookmarkHandler Create(BookmarkOptions options)
        {
            return new BookmarkHandler(options, logbook, pdfAConvertService, wordConvertService);
        }

        public IBookmarkHandler Create(BookmarkOptions options, IBookmarkFactory bookmarkFactory)
        {
            return new BookmarkHandler(options, bookmarkFactory, logbook, pdfAConvertService, wordConvertService);
        }
    }

    internal class TemporaryFilePointer
    {
        public IBookmark Bookmark { get; }
        public FileInfo TemporaryFile { get; }

        public TemporaryFilePointer(IBookmark bookmark, FileInfo temporaryFile)
        {
            Bookmark = bookmark;
            TemporaryFile = temporaryFile;
        }
    }
    internal class DefaultBookmarkFactory : IBookmarkFactory
    {
        public IBookmark Create(
            string title,
            IList<int> pages,
            int level,
            IList<IBookmark> children,
            Guid id,
            Guid parentId,
            string filePath,
            int index)
        {
            return new Bookmark(
                title: title,
                pages: pages,
                level: level,
                children: children,
                id: id,
                parentId: parentId,
                filePath: filePath,
                index: index);
        }
    }
    internal class DocumentBookmarkHandler : IDocumentBookmarkHandler
    {
        private readonly ILogbook logbook;
        private int pageCount;
        private readonly IBookmarkFactory bookmarkFactory;

        public List<IBookmark> Bookmarks { get; private set; }
        public string FilePath { get; }
        public string FileName { get; }
        public IBookmark FileAsBookmark
        {
            get => bookmarkFactory.Create(
                title: FileName,
                pages: new List<int>().Range(0, pageCount),
                level: 0,
                children: Bookmarks,
                id: new Guid(),
                parentId: Guid.Empty,
                filePath: FilePath,
                index: 0);
        }

        public DocumentBookmarkHandler(string filePath, ILogbook logbook)
            : this(filePath, new DefaultBookmarkFactory(), logbook) { }

        public DocumentBookmarkHandler(string filePath, IBookmarkFactory bookmarkFactory, ILogbook logbook)
        {
            this.logbook = logbook;
            FilePath = filePath;
            FileName = Path.GetFileNameWithoutExtension(filePath);
            this.bookmarkFactory = bookmarkFactory;
            Bookmarks = DocumentBookmarks(filePath);
        }

        public IList<int> SelectedPages()
        {
            HashSet<int> uniquePages = new HashSet<int>();
            Bookmarks.ForEach(b => b.Pages.ToList().ForEach(p => uniquePages.Add(p)));

            List<int> pageList = uniquePages.ToList();
            pageList.Sort();
            return pageList;
        }
        public void SelectAll() => Bookmarks.ForEach(b => b.IsSelected = true);

        public void Select(Guid id)
        {
            IBookmark? target = Bookmarks.Find(b => b.Id == id);
            if (target == null) return;

            target.IsSelected = true;

            Bookmarks.ForEach(b => { if (b.ParentId == id) b.IsSelected = true; });
        }

        public void Deselect(Guid id)
        {
            IBookmark? target = Bookmarks.Find(b => b.Id == id);
            if (target == null) return;

            target.IsSelected = false;

            Bookmarks.ForEach(b => { if (b.ParentId == id) b.IsSelected = false; });

            IBookmark? parent = Bookmarks.Find(b => b.Id == target.ParentId);
            if (parent != null) parent.IsSelected = false;
        }

        private List<IBookmark> DocumentBookmarks(string filePath)
        {
            PdfDocument document = new PdfDocument(new PdfReader(filePath));

            // Get source document outlines (bookmarks) and a tree of destinations in the document
            PdfNameTree destinationTree = document.GetCatalog().GetNameTree(PdfName.Dests);
            PdfOutline documentOutlines = document.GetOutlines(false);
            logbook.Write("Outlines retrieved.", LogLevel.Debug);

            List<IBookmark> initial = InitialBookmarksFromOutlines(documentOutlines, destinationTree.GetNames(), document, filePath);

            if (initial.Count < 1) return initial;

            pageCount = document.GetNumberOfPages();
            document.Close();

            logbook.Write("Levels and starting pages have been assigned.", LogLevel.Debug);

            List<IBookmark> finalBookmarks = GetAllPages(initial, pageCount);

            logbook.Write("All pages have been assigned.", LogLevel.Debug);

            return finalBookmarks;
        }

        private List<IBookmark> InitialBookmarksFromOutlines(
            PdfOutline outline,
            IDictionary<string, PdfObject> sourceNames,
            PdfDocument source,
            string filePath,
            int level = 0,
            Guid parentId = default)
        {
            if (outline == null) return new List<IBookmark>();

            Guid tempId = outline.GetDestination() == null ? Guid.Empty : new Guid();

            List<IBookmark> bookmarks = new List<IBookmark>();
            List<IBookmark> children = new List<IBookmark>();

            foreach (PdfOutline child in outline.GetAllChildren())
            {
                IList<IBookmark> childMarks = InitialBookmarksFromOutlines(
                    child,
                    sourceNames,
                    source,
                    filePath,
                    level + 1,
                    tempId);

                if (childMarks.Count < 1) continue;

                children.AddRange(childMarks);
            }

            if (outline.GetDestination() != null)
            {
                int startPage = source.GetPageNumber(
                    (PdfDictionary)outline.GetDestination().GetDestinationPage(sourceNames));

                IBookmark parent = bookmarkFactory.Create(
                    title: outline.GetTitle(),
                    pages: new List<int>() { startPage },
                    level: level,
                    children: children,
                    id: tempId,
                    parentId: parentId,
                    filePath: filePath,
                    index: 0);

                bookmarks.Add(parent);
            }

            bookmarks.AddRange(children);
            for (int i = 0; i < bookmarks.Count; i++) {
                bookmarks[i].Index = i;
            }

            return bookmarks;
        }

        /// <summary>
        /// Get all pages of bookmarks.
        /// </summary>
        /// <param name="bookmarks">Bookmarks to get pages for.</param>
        /// <param name="documentEndPage">Final page number of the document.</param>
        /// <returns>Bookmarks with pages.</returns>
        private List<IBookmark> GetAllPages(IList<IBookmark> bookmarks, int documentEndPage)
        {
            logbook.Write($"Retrieving all page ranges for all {nameof(IBookmark)}s.", LogLevel.Debug);

            if (bookmarks == null)
                return new List<IBookmark>();

            List<IBookmark> withEndPages = new List<IBookmark>();

            for (int i = 0; i < bookmarks.Count; i++)
            {
                IBookmark current = bookmarks[i];
                int endPage = documentEndPage;

                int maxStartPage = current.StartPage;
                if (current.Children.Count > 0)
                {
                    maxStartPage = current.Children[0].StartPage;
                    int maxIndex = 0;

                    for (int c = 0; c < current.Children.Count; c++)
                    {
                        IBookmark child = current.Children[c];
                        if (child.StartPage > maxStartPage || (child.StartPage == maxStartPage && c > maxIndex))
                        {
                            maxStartPage = child.StartPage;
                            maxIndex = c;
                        }
                    }
                }

                IBookmark? nextUp = null;
                for (int nu = i + 1; nu < bookmarks.Count; nu++)
                {
                    if (bookmarks[nu].Level <= current.Level)
                    {
                        nextUp = bookmarks[nu];
                        break;
                    }
                }

                if (nextUp != null)
                {
                    if (nextUp.StartPage == maxStartPage)
                    {
                        endPage = nextUp.StartPage;
                    }
                    else
                    {
                        endPage = nextUp.StartPage - 1;
                    }
                }

                List<int> allPages = new List<int>();
                for (int k = current.Pages[0]; k <= endPage; k++)
                {
                    allPages.Add(k);
                }

                withEndPages.Add(bookmarkFactory.Create(
                    title: current.Title,
                    pages: allPages,
                    level: current.Level,
                    children: current.Children,
                    id: current.Id,
                    parentId: current.ParentId,
                    filePath: current.FilePath,
                    index: current.Index));
            }

            logbook.Write($"Page ranges retrieved.", LogLevel.Debug);

            return withEndPages;
        }
    }
    internal class BookmarkHandler : WorkerBase<BookmarkHandler>, IBookmarkHandler
    {
        private readonly IPdfAConvertService pdfAConvertService;
        private readonly IWordConvertService wordConvertService;
        private readonly Utilities utilities;
        private readonly IBookmarkFactory bookmarkFactory;

        public event EventHandler<BookmarkOrFileExtractedEventArgs>? BookmarkOrFileExtracted;
        public event EventHandler<FileMergedEventArgs>? FileMerged;
        public event EventHandler<FilesConvertedToPdfAEventArgs>? FilesConverted;

        public ReorderCollection<IBookmark> Bookmarks { get; }
        public BookmarkOptions Options { get; set; }

        public BookmarkHandler(
            BookmarkOptions options,
            ILogbook logbook,
            IPdfAConvertService pdfAConvertService,
            IWordConvertService wordConvertService)
            : this(options, new DefaultBookmarkFactory(), logbook, pdfAConvertService, wordConvertService) { }

        public BookmarkHandler(
            BookmarkOptions options,
            IBookmarkFactory bookmarkFactory,
            ILogbook logbook,
            IPdfAConvertService pdfAConvertService,
            IWordConvertService wordConvertService) : base(logbook)
        {
            Bookmarks = new ReorderCollection<IBookmark>();
            Bookmarks.CanReorder = true;
            this.bookmarkFactory = bookmarkFactory;
            this.pdfAConvertService = pdfAConvertService;
            this.wordConvertService = wordConvertService;
            Options = options;
            utilities = new Utilities(logbook);
            PrepareCleanUp();
        }

        #region Commands
        private ICommand? moveUp;
        public ICommand MoveUp => moveUp ??= new MoveCommand(Bookmarks.ExecuteMoveUp);

        private ICommand? moveDown;
        public ICommand MoveDown => moveDown ??= new MoveCommand(Bookmarks.ExecuteMoveDown);

        private ICommand? moveLeft;
        public ICommand MoveLeft => moveLeft ??= new MoveCommand(Bookmarks.ExecuteMoveLeft);

        private ICommand? moveRight;
        public ICommand MoveRight => moveRight ??= new MoveCommand(Bookmarks.ExecuteMoveRight);
        #endregion

        public void AddFileAsBookmark(string filePath)
        {
            IDocumentBookmarkHandler bookmarkHandler = new DocumentBookmarkHandler(filePath, logbook.BaseLogbook);

            AddBookmark(bookmarkFactory.Create(
                title: Path.GetFileNameWithoutExtension(filePath),
                pages: bookmarkHandler.FileAsBookmark.Pages,
                level: 1,
                children: bookmarkHandler.Bookmarks,
                id: new Guid(),
                parentId: Guid.Empty,
                filePath: filePath,
                index: Bookmarks.Count
                ));
        }

        public void AddBookmark(IBookmark bookmark)
        {
            if (Bookmarks.Any(b => b.Id == bookmark.Id || b.Id == bookmark.ParentId)) return;

            IBookmark newBookmark = bookmarkFactory.Create(
                title: bookmark.Title,
                pages: bookmark.Pages.ToList(),
                level: bookmark.Level,
                children: bookmark.Children,
                id: bookmark.Id,
                parentId: bookmark.ParentId,
                filePath: bookmark.FilePath,
                index: bookmark.Index);

            Bookmarks.Push(newBookmark);

            foreach (IBookmark potentialChild in Bookmarks.ToList()) {
                if (potentialChild.ParentId == bookmark.Id) {
                    Bookmarks.Remove(potentialChild);
                }
            }
        }

        public void RemoveFile(string filePath)
        {
            Bookmarks.RemoveAll(b => b.FilePath == filePath);
        }

        private List<IBookmark> actualBookmarks()
        {
            List<IBookmark> indexed = new List<IBookmark>();
            for (int i = 0; i < Bookmarks.Count; i++) {
                IBookmark bookmark = Bookmarks[i];
                indexed.Add(bookmarkFactory.Create(
                    title: bookmark.Title,
                    pages: bookmark.Pages,
                    level: bookmark.Level,
                    children: bookmark.Children,
                    id: bookmark.Id,
                    parentId: bookmark.ParentId,
                    filePath: bookmark.FilePath,
                    index: i));
            }

            return indexed.Where(b => !b.IsFileless).ToList();
        }

        public async Task Merge()
        {
            if (Bookmarks.Count > 0) return;

            DirectoryInfo temporaryDirectory = utilities.CreateTemporaryDirectory();

            List<TemporaryFilePointer> temporaryFiles = new List<TemporaryFilePointer>();

            foreach (IBookmark pdfFile in Bookmarks.Where(b => b.FileNameExtension == ".pdf")) {
                string temporaryFilePath = Path.Combine(
                    temporaryDirectory.FullName, Path.GetRandomFileName());
                File.Copy(pdfFile.FilePath, temporaryFilePath);

                temporaryFiles.Add(new TemporaryFilePointer(pdfFile, new FileInfo(temporaryFilePath)));
                CreatedPaths.Add(new FileInfo(temporaryFilePath));
            }

            if (Options.ConvertWordDocuments)
            {
                List<IBookmark> wordFiles = Bookmarks
                    .Where(b => b.FileNameExtension == ".doc" || b.FileNameExtension == ".docx")
                    .ToList();

                List<WordConvertInput> convertInputs = wordFiles
                    .Select(b => new WordConvertInput(b.FilePath, Path.GetRandomFileName())).ToList();

                List<FileInfo> outputs = await wordConvertService.Convert(
                    convertInputs, temporaryDirectory, Options.Cancellation);

                for (int i = 0; i < outputs.Count; i++) {
                    temporaryFiles.Add(new TemporaryFilePointer(wordFiles[i], outputs[i]));
                }
            }

            if (CheckIfCancelledAndCleanUp(Options.Cancellation)) return;

            FileInfo targetFile = Options.Destination is FileInfo file
                ? file
                : new FileInfo(Path.Combine(Options.Destination.FullName, "Merge " + DateTime.Now));

            PdfDocument targetDocument = new PdfDocument(new PdfWriter(targetFile));
            OpenedDocuments.Add(targetDocument);

            PdfMerger merger = new PdfMerger(targetDocument).SetCloseSourceDocuments(true);

            List<IBookmark> targetDocumentBookmarks = new List<IBookmark>();

            foreach (IBookmark bookmark in Bookmarks) {
                List<IBookmark> children = new List<IBookmark>();
                TemporaryFilePointer? filePointer = temporaryFiles.Find(t => t.Bookmark.Id == bookmark.Id);

                if (filePointer != null) {
                    IDocumentBookmarkHandler bookmarkHandler =
                        new DocumentBookmarkHandler(
                            filePointer.TemporaryFile.FullName, 
                            bookmarkFactory, 
                            logbook.BaseLogbook);
                    children = bookmarkHandler.Bookmarks;
                }

                IBookmark? parent = null;
                for (int i = Bookmarks.Count - 1; i >= 0; i--) { 
                    if (Bookmarks[i].Level < bookmark.Level) {
                        parent = Bookmarks[i]; 
                        break;
                    }
                }

                IBookmark targetBookmark = bookmarkFactory.Create(
                    title: TemplateRename(bookmark),
                    pages: bookmark.Pages,
                    level: bookmark.Level,
                    children: children,
                    id: bookmark.Id,
                    parentId: parent != null ? parent.Id : bookmark.ParentId,
                    filePath: filePointer != null ? filePointer.TemporaryFile.FullName : bookmark.FilePath,
                    index: Bookmarks.IndexOf(bookmark));

                if (parent == null) {
                    targetDocumentBookmarks.Add(targetBookmark);
                } else {
                    parent.Children.Add(targetBookmark);
                }

                if (filePointer != null)
                {
                    PdfDocument sourceDocument = new PdfDocument(new PdfReader(filePointer.TemporaryFile.FullName));
                    OpenedDocuments.Add(sourceDocument);
                    merger.Merge(sourceDocument, 1, sourceDocument.GetNumberOfPages());
                }

                OnFileMerged(bookmark.IsFileless ? bookmark.Title : bookmark.FileName);
            }

            targetDocument.GetCatalog().Remove(PdfName.Outlines);
            AddBookmarksToDocument(targetDocumentBookmarks, targetDocument, targetFile.FullName, false);
            targetDocument.Close();

            OnFileMerged(targetFile.FullName, true);
        }

        public async Task Extract()
        {
            DirectoryInfo temporaryDirectory = utilities.CreateTemporaryDirectory();

            List<string> filePaths = actualBookmarks().Select(b => b.FilePath).Distinct().ToList();
            foreach (string p in filePaths)
            {
                PdfDocument sourceDocument = new PdfDocument(new PdfReader(p));
                OpenedDocuments.Add(sourceDocument);

                List<IBookmark> fileBookmarks = actualBookmarks().Where(b => b.FilePath == p).ToList();

                if (Options.Destination is DirectoryInfo) { // Extract as separate files
                    foreach (IBookmark bookmark in fileBookmarks) {
                        string targetFileName = TemplateRename(bookmark, temporaryDirectory).ReplaceIllegal();
                        
                        string targetFilePath = Path.Combine(
                            temporaryDirectory.FullName, 
                            targetFileName + ".pdf");

                        FileInfo targetFile = new FileInfo(targetFilePath);

                        PdfDocument result = await ExtractPages(targetFile, bookmark.Pages, sourceDocument, Options);
                        result.GetCatalog().Remove(PdfName.Outlines);

                        IList<IBookmark> bookmarksToAdd = bookmark.Children.Count < 1
                            ? new List<IBookmark>()
                            : bookmark.Children.Any(c => c.StartPage <= bookmark.StartPage)
                                ? bookmark.Children
                                : new List<IBookmark>() { bookmark };

                        AddBookmarksToDocument(bookmarksToAdd.ToList(), result, p, false);

                        result.Close();

                        OnBookmarkOrFileExtracted(bookmark.Title, bookmark == fileBookmarks.Last());

                        if (CheckIfCancelledAndCleanUp(Options.Cancellation)) return;
                    }
                }
                else { // ... or as a single file 
                    HashSet<int> uniquePages = new HashSet<int>();
                    fileBookmarks.ForEach(b => b.Pages.ToList().ForEach(p => uniquePages.Add(p)));
                    List<int> allPages = uniquePages.ToList();
                    allPages.Sort();

                    FileInfo destinationFile = new FileInfo(Path.Combine(temporaryDirectory.FullName,
                        Path.GetFileName(p)));

                    PdfDocument result = await ExtractPages(destinationFile, allPages, sourceDocument, Options);
                    result.GetCatalog().Remove(PdfName.Outlines);

                    fileBookmarks.ForEach(b => b.Title = TemplateRename(b));

                    AddBookmarksToDocument(fileBookmarks, result, p, Options.GroupByFiles);

                    result.Close();
                }

                sourceDocument.Close();

                if (Options.Destination is FileInfo) {
                    OnBookmarkOrFileExtracted(Path.GetFileNameWithoutExtension(p), p == filePaths.Last());
                }

                CheckIfCancelledAndCleanUp(Options.Cancellation);
            };

            if (Options.Destination is FileInfo file)
            {
                List<FileInfo> products = temporaryDirectory.GetFiles().ToList();

                string mergeFilePath = Path.Combine(temporaryDirectory.FullName, file.Name);
                PdfDocument targetDocument = new PdfDocument(new PdfWriter(mergeFilePath));

                PdfMerger merger = new PdfMerger(targetDocument, false, true);

                foreach (FileInfo mergeFile in products)
                {
                    PdfDocument mergeSourceDocument = new PdfDocument(new PdfReader(mergeFile.FullName));
                    merger.Merge(mergeSourceDocument, 1, mergeSourceDocument.GetNumberOfPages());
                    mergeSourceDocument.Close();
                    mergeFile.Delete();

                    OnFileMerged(mergeFile.FullName);
                }
            }

            DirectoryInfo? targetDirectory = Options.Destination is DirectoryInfo
                ? Options.Destination as DirectoryInfo
                : Options.Destination is FileInfo finalFile
                    ? finalFile.Directory
                    : null;

            if (targetDirectory == null) throw new ArgumentException("Target directory was not a valid directory.");

            targetDirectory.Create();

            if (Options.ConvertToPdfA) {
                bool success = await pdfAConvertService.Convert(temporaryDirectory, targetDirectory, Options.Cancellation);

                if (!success) {
                    temporaryDirectory
                        .GetFiles()
                        .ToList()
                        .ForEach(f => f
                            .CopyTo(Path.Combine(targetDirectory.FullName, f.Name)));
                }

                OnFilesConverted(success, targetDirectory.FullName);
            }

            temporaryDirectory.Delete(true);
        }

        private string TemplateRename(IBookmark bookmark, DirectoryInfo? targetDirectory = null)
        {
            if (Options.NameTemplate == null) return bookmark.Title;

            int numberCount = actualBookmarks().Last().Index.ToString().Length;
            int zeroCount = numberCount - bookmark.Index.ToString().Length;
            string countString = zeroCount > 0
                ? string.Concat(Enumerable.Repeat("0", zeroCount)) + bookmark.Index.ToString()
                : bookmark.Index.ToString();

            string name = Options.NameTemplate;
            name = name
                .ReplacePlaceholder(Placeholders.Bookmark, bookmark.Title)
                .ReplacePlaceholder(Placeholders.File, bookmark.FileName)
                .ReplacePlaceholder(Placeholders.Number, countString);

            if (targetDirectory == null) return name;

            int previousCount = targetDirectory
                .GetFiles()
                .Select(f => Path.GetFileNameWithoutExtension(f.FullName))
                .Where(n => n == name)
                .Count();
            if (previousCount > 0)
            {
                name = $"{name} {previousCount + 1}";
            }

            return name;
        }

        private void OnFilesConverted(bool faulted, string destinationDirectoryPath)
        {
            FilesConvertedToPdfAEventArgs e = new FilesConvertedToPdfAEventArgs(faulted, destinationDirectoryPath);
            FilesConverted?.Invoke(this, e);
        }

        private void OnBookmarkOrFileExtracted(string name, bool isDone)
        {
            BookmarkOrFileExtractedEventArgs e =
                new BookmarkOrFileExtractedEventArgs(name, isDone);
            BookmarkOrFileExtracted?.Invoke(this, e);
        }

        private void OnFileMerged(string filePath, bool isDone = false)
        {
            FileMergedEventArgs e = new FileMergedEventArgs(filePath, isDone);
            FileMerged?.Invoke(this, e);
        }

        private void AddBookmarksToDocument(
            List<IBookmark> bookmarks, 
            PdfDocument targetDocument, 
            string filePath, 
            bool groupByFiles)
        {
            logbook.Write($"Adjusting {nameof(IBookmark)}s for extraction.", LogLevel.Debug);

            List<IBookmark> adjusted = new List<IBookmark>();

            int currentPage = 1;

            foreach (IBookmark bookmark in bookmarks)
            {
                List<IBookmark> adjustedChildren = bookmark.Children.Select(b => 
                    bookmarkFactory.Create(
                        title: b.Title,
                        pages: new List<int>().Range(currentPage + (b.StartPage - bookmark.StartPage), b.Pages.Count),
                        level: b.Level,
                        children: new List<IBookmark>(),
                        id: b.Id,
                        parentId: b.ParentId,
                        filePath: b.FilePath,
                        index: b.Index)
                ).ToList();

                adjusted.Add(bookmarkFactory.Create(
                    title: bookmark.Title,
                    pages: new List<int>().Range(currentPage, bookmark.Pages.Count),
                    level: bookmark.Level,
                    children: adjustedChildren,
                    id: bookmark.Id,
                    parentId: bookmark.ParentId,
                    filePath: bookmark.FilePath,
                    index: bookmark.Index));

                currentPage += bookmark.Pages.Count;
            }

            logbook.Write("Initializing outlines.", LogLevel.Debug);

            targetDocument.InitializeOutlines();
            PdfOutline root = groupByFiles
                ? targetDocument.GetOutlines(true).AddOutline(Path.GetFileNameWithoutExtension(filePath))
                : targetDocument.GetOutlines(true);

            logbook.Write($"Saving all {nameof(IBookmark)}s in hierarchy.", LogLevel.Debug);

            AddOutlinesRecursively(root, adjusted, targetDocument);
        }

        private void AddOutlinesRecursively(
            PdfOutline parentOutline, 
            IList<IBookmark> children, 
            PdfDocument targetDocument,
            int? parentLevel = null)
        {
            foreach (IBookmark directChild in parentLevel == null ? children : children.Where(c => c.Level == parentLevel + 1))
            {
                PdfOutline childOutline = parentOutline.AddOutline(directChild.Title);
                childOutline.AddDestination(PdfExplicitDestination.CreateFit(targetDocument.GetPage(directChild.StartPage)));
                AddOutlinesRecursively(childOutline, directChild.Children, targetDocument, directChild.Level);
            }
        }

        private async Task<PdfDocument> ExtractPages(
            FileInfo destinationFile,
            IList<int> pages,
            PdfDocument source,
            BookmarkOptions options)
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
    }
}
