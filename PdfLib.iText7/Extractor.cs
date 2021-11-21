using System;
using System.Collections.Generic;
using System.Linq;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using CX.PdfLib.Extensions;
using System.IO;
using PdfLib.iText7;

namespace CX.PdfLib.Implementation
{
    /// <summary>
    /// Implementation of <see cref="IExtractor"/> service
    /// </summary>
    public class Extractor : IExtractor
    {
        /// <summary>
        /// Get new instance as service
        /// </summary>
        /// <returns>IExtractor service</returns>
        public static IExtractor GetService() => new Extractor();

        public void Extract(string sourceFile, DirectoryInfo destDirectory, IEnumerable<ILeveledBookmark> extractables)
        {
            var doc = new PdfDocument(new PdfReader(sourceFile));

            foreach (ILeveledBookmark bm in extractables)
            {
                // Extract pages into a file
                FileInfo destFile = new FileInfo(Path.Combine(destDirectory.FullName,
                    bm.Title.ReplaceIllegal() + ".pdf"));
                destDirectory.Create();
                var split = new ExtSplitter(doc, pageRange => new PdfWriter(destFile));
                var result = split.ExtractPages(bm.Pages);

                // Remove bookmarks copied with merging
                result.GetCatalog().Remove(PdfName.Outlines);
                // Find children of the current bookmark in the original list and adjust their start pages
                // to match the new document
                IList<ILeveledBookmark> children = Bookmarker.AdjustBookmarksExtract(extractables.ToList(), bm.Pages);
                // Remove the bookmark being extracted from previous list
                children.RemoveAt(0);
                // If there are children, adjust their levels to match the new document tree
                if (children.Count > 0)
                {
                    IList<ILeveledBookmark> levelAdjustedChildren = children.AdjustLevels(1 - children[0].Level);
                    Bookmarker.AddLeveledBookmarks(Bookmarker.AdjustBookmarksExtract(levelAdjustedChildren, bm.Pages), result);
                }
                result.Close();
            }

            doc.Close();
        }
        /// <summary>
        /// Preserves original document bookmarks
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destFile"></param>
        /// <param name="extractables"></param>
        public void Extract(string sourceFile, FileInfo destFile, IEnumerable<ILeveledBookmark> extractables)
        {
            var doc = new PdfDocument(new PdfReader(sourceFile));

            // Get all pages in all the ranges
            List<int> pages = new List<int>();
            foreach (ILeveledBookmark bm in extractables)
            {
                pages.AddRange(bm.Pages);
            }

            // Extract pages into a file
            var split = new ExtSplitter(doc, pageRange => new PdfWriter(destFile.FullName));
            var result = split.ExtractPages(pages);

            // Add bookmarks pointing to extracted pages from the original document
            IList<ILeveledBookmark> sourceBookmarks = Bookmarker.FindLeveledBookmarks(doc);
            doc.Close();
            // Remove bookmarks copied with merging
            result.GetCatalog().Remove(PdfName.Outlines);
            // Adjust pages to match the new document and adjust levels
            IList<ILeveledBookmark> adjusted = Bookmarker.AdjustBookmarksExtract(sourceBookmarks, pages);
            if (adjusted.Count > 0)
            {
                Bookmarker.AddLeveledBookmarks(adjusted.AdjustLevels(1 - adjusted[0].Level), result);
            }

            result.Close();
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
