using CX.PdfLib.Common;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
using iText.Kernel.Pdf;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CX.PdfLib.Extensions;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.Threading.Tasks;

namespace CX.PdfLib.iText7
{
    public class Manipulator : IManipulator
    {
        private IBookmarker bookmarker;
        private IConverter converter;
        private IExtractor extractor;
        private IMerger merger;
        private ISigner signer;

        public Manipulator(IBookmarker bookmarker, IConverter converter, IExtractor extractor,
            IMerger merger, ISigner signer)
        {
            this.bookmarker = bookmarker;
            this.converter = converter;
            this.extractor = extractor;
            this.merger = merger;
            this.signer = signer;
        }

        #region EXTRACTION
        public void Extract(string sourceFile, DirectoryInfo destDirectory,
            IEnumerable<ILeveledBookmark> ranges)
            => extractor.Extract(sourceFile, destDirectory, ranges);
        public void Extract(string sourceFile, FileInfo destFile,
            IEnumerable<ILeveledBookmark> ranges)
            => extractor.Extract(sourceFile, destFile, ranges);
        public async Task ExtractAsync(string sourceFile, FileInfo destFile,
            IEnumerable<ILeveledBookmark> ranges, IProgress<ProgressReport> progress)
        {
            await Task.Run(() => extractor.Extract(sourceFile, destFile, ranges, progress));
        }
        public async Task ExtractAsync(string sourceFile, DirectoryInfo destDirectory,
            IEnumerable<ILeveledBookmark> ranges, IProgress<ProgressReport> progress)
        {
            await Task.Run(() => extractor.Extract(sourceFile, destDirectory, ranges, progress));
        }
        #endregion

        #region BOOKMARKS
        public void AddBookmarks(IList<ILeveledBookmark> bookmarks, string documentPath)
            => bookmarker.AddBookmarks(bookmarks, documentPath);
        public async Task AddBookmarksAsync(IList<ILeveledBookmark> bookmarks, string documentPath)
        {
            await Task.Run(() => bookmarker.AddBookmarks(bookmarks, documentPath));
        }
        public IList<ILeveledBookmark> FindBookmarks(string sourcePdf)
           => bookmarker.FindBookmarks(sourcePdf);
        public async Task<IList<ILeveledBookmark>> FindBookmarksAsync(string sourcePdf)
        {
            return await Task.Run(() => bookmarker.FindBookmarks(sourcePdf));
        }
        #endregion

        #region SIGNATURE
        public void RemoveSignature(string sourcePath, DirectoryInfo destinationDirectory, string postFix)
            => signer.RemoveSignature(sourcePath, destinationDirectory, postFix);
        public async Task RemoveSignatureAsync(string sourcePath, DirectoryInfo destinationDirectory, string postFix)
        {
            await Task.Run(() => signer.RemoveSignature(sourcePath, destinationDirectory, postFix));
        }
        public void RemoveSignature(string sourcePath, FileInfo outputFile)
            => signer.RemoveSignature(sourcePath, outputFile);
        public async Task RemoveSignatureAsync(string sourcePath, FileInfo outputFile)
        {
            await Task.Run(() => signer.RemoveSignature(sourcePath, outputFile));
        }
        public void RemoveSignature(string[] sourcePaths, DirectoryInfo destinationDirectory, string postFix)
            => signer.RemoveSignature(sourcePaths, destinationDirectory, postFix);
        public async Task RemoveSignatureAsync(string[] sourcePaths, DirectoryInfo destinationDirectory, string postFix)
        {
            await Task.Run(() => signer.RemoveSignature(sourcePaths, destinationDirectory, postFix));
        }
        #endregion

        #region CONVERT
        public string Convert(string filePath, string outputDirectory)
            => converter.Convert(filePath, outputDirectory);
        public async Task<string> ConvertAsync(string filePath, string outputDirectory)
        {
            return await Task.Run(() => converter.Convert(filePath, outputDirectory));
        }
        public IList<string> Convert(IList<string> filePaths, string outputDirectory)
            => converter.Convert(filePaths, outputDirectory);
        public async Task<IList<string>> ConvertAsync(IList<string> filePaths, string outputDirectory)
        {
            return await Task.Run(() => converter.Convert(filePaths, outputDirectory));
        }

        #endregion

        #region MERGE
        public IList<int> Merge(IList<string> sourcePaths, string outputPath)
            => merger.Merge(sourcePaths, outputPath);
        #endregion

        public void MergeWithBookmarks(IList<IMergeInput> inputs, string outputPath)
        {
            PdfDocument doc = new PdfDocument(new PdfWriter(outputPath));
            IList<string> converted = converter.Convert(GetMergePaths(inputs), null);
            var (startPages, outputPageCount) = DoMerge(converted, doc);

            List<ILeveledBookmark> bookmarks = new List<ILeveledBookmark>();
            for (int i = 0; i < inputs.Count; i++)
            {
                IMergeInput current = inputs[i];
                bookmarks.Add(new LeveledBookmark(current.Level, current.Title,
                    startPages[i], 1));
                if (current.FilePath != null && Path.GetExtension(current.FilePath).ToLower() == ".pdf")
                {
                    IList<ILeveledBookmark> leveledOriginal = FindBookmarks(current.FilePath)
                        .AdjustLevels(current.Level);
                    bookmarks.AddRange(Bookmarker.AdjustBookmarksMerge(leveledOriginal, startPages[i]));
                }
            }

            doc.GetCatalog().Remove(PdfName.Outlines);
            Bookmarker.AddLeveledBookmarks(Bookmarker.GetAllPages(bookmarks, outputPageCount), doc);

            Document document = new Document(doc);
            AddPageNumbers(document);
            document.Close();
        }

        private List<string> GetMergePaths(IList<IMergeInput> inputs)
        {
            return inputs.Select(x => x.FilePath).ToList();
        }
        // Returns a list of start pages of merged documents in the output file and total
        // number of pages in the final document
        private (IList<int> startPages, int outputPageCount) DoMerge(IList<string> mergePaths, PdfDocument doc)
        {
            IList<int> startPages = Merger.Merge(mergePaths, doc);
            int outputPageCount = doc.GetNumberOfPages();
            return (startPages, outputPageCount);
        }
        // Add page numbers to a pdf document
        private void AddPageNumbers(Document doc)
        {
            for (int i = 1; i <= doc.GetPdfDocument().GetNumberOfPages(); i++)
            {
                doc.ShowTextAligned(new Paragraph($"{i}"), 520, 780, i, iText.Layout.Properties.TextAlignment.RIGHT,
                    iText.Layout.Properties.VerticalAlignment.TOP, 0);
            }
        }
    }
}
