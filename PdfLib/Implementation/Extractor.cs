using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using CX.PdfLib.Extensions;
using System.IO;
using CX.PdfLib.Implementation.Data;

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

        public void Extract(string sourceFile, DirectoryInfo destDirectory, IEnumerable<IExtractRange> ranges)
        {
            var doc = new PdfDocument(new PdfReader(sourceFile));

            foreach (IExtractRange range in ranges)
            {
                // Extract pages into a file
                FileInfo destFile = new FileInfo(Path.Combine(destDirectory.FullName,
                    range.Name.ReplaceIllegal() + ".pdf"));
                destDirectory.Create();
                var split = new ExtSplitter(doc, pageRange => new PdfWriter(destFile));
                var result = split.ExtractPages(range.Pages);

                result.Close();
            }

            doc.Close();
        }
        /// <summary>
        /// Preserves original document bookmarks
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destFile"></param>
        /// <param name="ranges"></param>
        public void Extract(string sourceFile, FileInfo destFile, IEnumerable<IExtractRange> ranges)
        {
            var doc = new PdfDocument(new PdfReader(sourceFile));

            // Get all pages in all the ranges
            List<int> pages = new List<int>();
            foreach (IExtractRange range in ranges)
            {
                pages.AddRange(range.Pages);
            }

            // Extract pages into a file
            var split = new ExtSplitter(doc, pageRange => new PdfWriter(destFile));
            var result = split.ExtractPages(pages);

            // Add bookmarks pointing to extracted pages from the original document
            IList<ILeveledBookmark> sourceBookmarks = Bookmarker.FindLeveledBookmarks(doc);
            Bookmarker.AddLeveledBookmarks(Bookmarker.AdjustBookmarksExtract(sourceBookmarks, pages), result);
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
                return ExtractPageRange(new PageRange(RangesAsString(pages)));
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
