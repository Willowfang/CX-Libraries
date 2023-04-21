using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WF.PdfLib.Common;
using WF.PdfLib.Services.Data;

namespace WF.PdfLib.Services.Bookmarks
{
    /// <summary>
    /// Service container for a bookmark in a pdf-document.
    /// </summary>
    public interface IBookmark : ILeveledItem, ISelectable, IIndexed
    {
        /// <summary>
        /// Unique id for this bookmark.
        /// </summary>
        public Guid Id { get; }
        /// <summary>
        /// Name of the bookmark.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Pages in the bookmark page range.
        /// </summary>
        public IList<int> Pages { get; }

        /// <summary>
        /// The first page in the bookmark page range.
        /// </summary>
        public int StartPage { get; }

        /// <summary>
        /// Get last page in the bookmark page range.
        /// </summary>
        public int EndPage { get; }

        /// <summary>
        /// All children (recursively) of this particular bookmark.
        /// </summary>
        public IList<IBookmark> Children { get; }

        /// <summary>
        /// Id of this bookmark's parent.
        /// </summary>
        public Guid ParentId { get; }

        /// <summary>
        /// Path to the file containing this bookmark.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Name of the file containing this bookmark.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// The extension of this bookmark's filename.
        /// </summary>
        public string FileNameExtension { get; }

        /// <summary>
        /// The name of this file with extension.
        /// </summary>
        public string FileNameWithExtension { get; }

        /// <summary>
        /// Page range formatted as a string.
        /// </summary>
        public string FormattedRange { get; }

        public bool IsFileless { get; set; }
    }

    public interface IDocumentBookmarkHandler
    {
        /// <summary>
        /// A list of the bookmarks contained in this handler.
        /// </summary>
        public List<IBookmark> Bookmarks { get; }

        /// <summary>
        /// Path of the source file.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Name of the source file without extension.
        /// </summary>
        public string FileName { get; }

        public IBookmark FileAsBookmark { get; }

        /// <summary>
        /// Return all pages that have been selected.
        /// </summary>
        public IList<int> SelectedPages();

        /// <summary>
        /// Select all bookmarks.
        /// </summary>
        public void SelectAll();

        /// <summary>
        /// Select a bookmark and all of its children.
        /// </summary>
        /// <param name="id">Id of the bookmark to select.</param>
        public void Select(Guid id);

        /// <summary>
        /// Deselect a bookmark and all of its children. Deselect this bookmarks parent, if there is one.
        /// </summary>
        /// <param name="id">Id of the bookmark to deselect.</param>
        public void Deselect(Guid id);
    }

    public interface IDocumentBookmarkHandlerFactory
    {
        public IDocumentBookmarkHandler Create(string filePath);
        public IDocumentBookmarkHandler Create(string filePath, IBookmarkFactory bookmarkFactory);
    }

    public class BookmarkOptions
    {
        /// <summary>
        /// Where to extract the bookmarks.
        /// </summary>
        public FileSystemInfo Destination { get; set; }

        /// <summary>
        /// Cancellation token for this operation. Is ignored when serializing (is not serializable). Default is
        /// <see cref="CancellationToken.None"/>.
        /// </summary>
        [JsonIgnore]
        public CancellationToken Cancellation { get; set; }

        /// <summary>
        /// Chosen option for dealing with annotations. Default is <see cref="AnnotationOption.Keep"/>.
        /// </summary>
        public AnnotationOption Annotations { get; set; }

        /// <summary>
        /// The users, whose annotations will be removed from the products. Default is an empty list.
        /// </summary>
        public IEnumerable<string> AnnotationUsersToRemove { get; set; }

        /// <summary>
        /// If true, convert products to pdf/A. Requires PDF-Tools to be present. Default is false.
        /// </summary>
        public bool ConvertToPdfA { get; set; }

        /// <summary>
        /// If true, when extracting into a single file, bookmarks will be grouped by file names
        /// instead of sequential order. Default is false.
        /// </summary>
        public bool GroupByFiles { get; set; }

        /// <summary>
        /// Name template for product bookmarks or files. If null, bookmarks or files will be named by title or
        /// filename. Default is null.
        /// </summary>
        public string? NameTemplate { get; set; }

        /// <summary>
        /// If true, page numbers will be added to products. Default is false.
        /// </summary>
        public bool AddPageNumbers { get; set; }

        /// <summary>
        /// If true, when merging files, word files will be converted to pdf prior to merging. If false, they
        /// will be ignored. Default is false.
        /// </summary>
        public bool ConvertWordDocuments { get; set; }

        public BookmarkOptions(FileSystemInfo destination)
        {
            Destination = destination;
            Cancellation = CancellationToken.None;
            Annotations = AnnotationOption.Keep;
            AnnotationUsersToRemove = new List<string>();
        }
    }

    public interface IBookmarkHandler
    {
        public event EventHandler<BookmarkOrFileExtractedEventArgs> BookmarkOrFileExtracted;

        public event EventHandler<FileMergedEventArgs> FileMerged;

        public event EventHandler<FilesConvertedToPdfAEventArgs> FilesConverted;

        public ReorderCollection<IBookmark> Bookmarks { get; }

        public BookmarkOptions Options { get; set; }

        /// <summary>
        /// Command for moving an item up.
        /// </summary>
        public ICommand MoveUp { get; }
        /// <summary>
        /// Command for moving an item down.
        /// </summary>
        public ICommand MoveDown { get; }
        /// <summary>
        /// Command for decreasing an item's level.
        /// </summary>
        public ICommand MoveLeft { get; }
        /// <summary>
        /// Command for increasing an items level.
        /// </summary>
        public ICommand MoveRight { get; }
        public void AddFileAsBookmark(string filePath);
        public void AddBookmark(IBookmark bookmark);
        public void RemoveFile(string filePath);
        public Task Merge();
        public Task Extract();
    }

    public interface IBookmarkHandlerFactory
    {
        public IBookmarkHandler Create(BookmarkOptions options);
        public IBookmarkHandler Create(BookmarkOptions options, IBookmarkFactory bookmarkFactory);
    }

    public interface IBookmarkFactory
    {
        public IBookmark Create(
            string title,
            IList<int> pages,
            int level,
            IList<IBookmark> children,
            Guid id,
            Guid parentId,
            string filePath,
            int index);
    }
}
