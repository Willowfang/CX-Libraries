using CX.PdfLib.Common;
using CX.PdfLib.Services.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CX.PdfLib.Services
{
    public interface IManipulator
    {
        #region EXTRACTION
        /// <summary>
        /// Extract multiple page ranges into separate files
        /// </summary>
        /// <param name="sourceFile">Path of the file to extract from</param>
        /// <param name="destDirectory">Directory to extract the files to</param>
        /// <param name="ranges">Ranges to extract</param>
        public void Extract(string sourceFile, DirectoryInfo destDirectory, IEnumerable<ILeveledBookmark> ranges);
        /// <summary>
        /// Extract multiple page ranges into one file
        /// </summary>
        /// <param name="sourceFile">File to extract from</param>
        /// <param name="destFile">File to extract into</param>
        /// <param name="ranges">Page ranges to extract</param>
        public void Extract(string sourceFile, FileInfo destFile, IEnumerable<ILeveledBookmark> ranges);
        /// <summary>
        /// Extract multiple page ranges into separate files asynchronously
        /// </summary>
        /// <param name="sourceFile">Path of the file to extract from</param>
        /// <param name="destDirectory">Directory to extract the files to</param>
        /// <param name="ranges">Ranges to extract</param>
        /// <param name="progress">Optional progress reporter</param>
        public Task ExtractAsync(string sourceFile, DirectoryInfo destDirectory, IEnumerable<ILeveledBookmark> ranges,
            IProgress<ProgressReport> progress = null);
        /// <summary>
        /// Extract multiple page ranges into one file asynchronously
        /// </summary>
        /// <param name="sourceFile">File to extract from</param>
        /// <param name="destFile">File to extract into</param>
        /// <param name="ranges">Page ranges to extract</param>
        /// <param name="progress">Optional progress reporter</param>
        public Task ExtractAsync(string sourceFile, FileInfo destFile, IEnumerable<ILeveledBookmark> ranges,
            IProgress<ProgressReport> progress = null);
        #endregion

        #region BOOKMARKS
        public IList<ILeveledBookmark> FindBookmarks(string sourcePdf);
        public Task<IList<ILeveledBookmark>> FindBookmarksAsync(string sourcePdf);
        public void AddBookmarks(IList<ILeveledBookmark> bookmarks, string documentPath);
        public Task AddBookmarksAsync(IList<ILeveledBookmark> bookmarks, string documentPath);
        #endregion

        #region SIGNATURE
        public void RemoveSignature(string sourcePath, DirectoryInfo destinationDirectory, string postFix);
        public Task RemoveSignatureAsync(string sourcePath, DirectoryInfo destinationDirectory, string postFix);
        public void RemoveSignature(string sourcePath, FileInfo outputFile);
        public Task RemoveSignatureAsync(string sourcePath, FileInfo outputFile);
        public void RemoveSignature(string[] sourcePaths, DirectoryInfo destinationDirectory, string postFix);
        public Task RemoveSignatureAsync(string[] sourcePaths, DirectoryInfo destinationDirectory, string postFix);
        #endregion

        #region CONVERSION
        public string Convert(string filePath, string outputDirectory);
        public Task<string> ConvertAsync(string filePath, string outputDirectory);
        public IList<string> Convert(IList<string> filePaths, string outputDirectory);
        public Task<IList<string>> ConvertAsync(IList<string> filePaths, string outputDirectory);
        #endregion

        #region MERGING
        public IList<int> Merge(IList<string> sourcePaths, string outputPath);
        public Task<IList<int>> MergeAsync(IList<string> sourcePaths, string outputPath);
        #endregion

        /// <summary>
        /// Merge documents into one pdf. Create top-level bookmarks for merged
        /// files and assign them given titles. Retain all bookmarks from original documents
        /// with adjusted destination pages.
        /// </summary>
        /// <param name="inputs">Inputs for the merge, includes merging files
        /// and titles</param>
        /// <param name="outputPath">Output file path</param>
        /// <param name="addPageNumbers">If true, add page numbers to new document</param>
        public void MergeWithBookmarks(IList<IMergeInput> inputs, string outputPath, bool addPageNumbers);
        public Task MergeWithBookmarksAsync(IList<IMergeInput> inputs, string outputPath,
            bool addPageNumbers, IProgress<ProgressReport> progress = null);
    }
}
