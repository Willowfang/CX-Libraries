using CX.PdfLib.Common;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CX.PdfLib.iText7
{
    public class Bookmarker : IBookmarker
    {
        /// <summary>
        /// Get all bookmarks from a document
        /// </summary>
        /// <param name="sourcePdf">Source document path</param>
        /// <returns></returns>
        public IList<ILeveledBookmark> FindBookmarks(string sourcePdf)
        {
            return FindLeveledBookmarks(new PdfDocument(new PdfReader(sourcePdf)));
        }
        /// <summary>
        /// Find and return bookmarks with their levels
        /// </summary>
        /// <param name="sourceDoc">Document to search</param>
        /// <returns>A list of leveled bookmarks</returns>
        internal static IList<ILeveledBookmark> FindLeveledBookmarks(PdfDocument sourceDoc)
        {
            // Get source document outlines (bookmarks) and a tree of destinations in the document
            PdfNameTree destTree = sourceDoc.GetCatalog().GetNameTree(PdfName.Dests);
            PdfOutline outlines = sourceDoc.GetOutlines(false);

            // Get bookmarks with their levels and starting pages
            IList<ILeveledBookmark> foundBookmarks = GetBookmarks(outlines, destTree.GetNames(), sourceDoc);
            int documentEndPage = sourceDoc.GetNumberOfPages();
            sourceDoc.Close();

            // Get all other pages (in addition to starting page) of all the bookmarks
            return GetAllPages(foundBookmarks, documentEndPage);
        }

        /// <summary>
        /// Add bookmarks to a document
        /// </summary>
        /// <param name="bookmarks">Bookmarks to add</param>
        /// <param name="documentPath">Destination document path</param>
        public void AddBookmarks(IList<ILeveledBookmark> bookmarks, string documentPath)
        {
            PdfDocument product = new PdfDocument(new PdfWriter(documentPath));
            AddLeveledBookmarks(bookmarks, product);
            product.Close();
        }
        internal static void AddLeveledBookmarks(IList<ILeveledBookmark> bookmarks, PdfDocument product)
        {
            product.InitializeOutlines();
            PdfOutline root = product.GetOutlines(true);

            // Created outlines connected to bookmarks given as argument
            List<Tuple<PdfOutline, ILeveledBookmark>> outlines = new List<Tuple<PdfOutline, ILeveledBookmark>>();

            // Iterate, in order, through all bookmarks and save them
            // in their right place in the hierarchy
            foreach (ILeveledBookmark current in bookmarks)
            {
                PdfOutline addOutline = null;

                // If bookmark is a root level outline, assign immediately
                if (current.Level == 1)
                {
                    addOutline = root.AddOutline(current.Title);
                }
                // If bookmarks is not a root level bookmark, find last outline
                // that is smaller level than current (to create a tree-like structure)
                else
                {
                    for (int parentLevel = current.Level - 1; parentLevel >= 1; parentLevel--)
                    {
                        for (int i = outlines.Count; i-- > 0;)
                        {
                            if (outlines[i].Item2.Level == parentLevel)
                            {
                                addOutline = outlines[i].Item1.AddOutline(current.Title);
                                break;
                            }
                        }

                        if (addOutline != null)
                            break;
                    }
                }
                // If the bookmark is not a root-level bookmark but no parent has been found,
                // assign the bookmark as a root-level bookmark
                addOutline ??= root.AddOutline(current.Title);

                outlines.Add(Tuple.Create(addOutline, current));
                // Add destination page number for currently processed outline (otherwise no navigation
                // will happen by clicking bookmark name)
                if (current.Pages[0] <= product.GetNumberOfPages())
                    addOutline.AddDestination(PdfExplicitDestination.CreateFit(product.GetPage(current.StartPage)));
            }
        }

        internal static IList<ILeveledBookmark> AdjustBookmarksMerge(IList<ILeveledBookmark> originalBookmarks,
            int startPageInNewDocument)
        {
            List<ILeveledBookmark> adjustedBookmarks = new List<ILeveledBookmark>();

            foreach (ILeveledBookmark original in originalBookmarks)
            {
                adjustedBookmarks.Add(new LeveledBookmark(original.Level, original.Title,
                    startPageInNewDocument + original.StartPage - 1, original.Pages.Count));
            }

            return adjustedBookmarks;
        }
        internal static IList<ILeveledBookmark> AdjustBookmarksExtract(IList<ILeveledBookmark> sourceBookmarks,
            IList<int> extractedPages)
        {
            List<ILeveledBookmark> correctedBookmarks = new List<ILeveledBookmark>();

            for (int i = 0; i < extractedPages.Count; i++)
            {
                // Search for bookmarks with the current page as destination
                foreach (ILeveledBookmark bookmark in sourceBookmarks)
                {
                    if (extractedPages[i] == bookmark.StartPage)
                    {
                        // The corrected destination is the next page after all previous extracted pages
                        int correctedFirstPage = extractedPages.Count(x => x <= extractedPages[i]);
                        correctedBookmarks.Add(new LeveledBookmark(bookmark.Level,
                            bookmark.Title, correctedFirstPage, bookmark.Pages.Count));
                    }
                }
            }

            return correctedBookmarks;
        }

        private static IList<ILeveledBookmark> GetBookmarks(PdfOutline outline,
            IDictionary<string, PdfObject> sourceNames, PdfDocument sourceDocument, int level = 0)
        {
            if (outline == null) return null;

            List<ILeveledBookmark> bookmarks = new List<ILeveledBookmark>();

            if (outline.GetDestination() != null)
            {
                int startPage = sourceDocument.GetPageNumber(
                    (PdfDictionary)outline.GetDestination().GetDestinationPage(sourceNames));
                bookmarks.Add(new LeveledBookmark(level, outline.GetTitle(), new List<int>() { startPage }));
            }

            foreach (PdfOutline child in outline.GetAllChildren())
            {
                var childMarks = GetBookmarks(child, sourceNames, sourceDocument, level + 1);
                if (childMarks != null)
                    bookmarks.AddRange(childMarks);
            }

            return bookmarks;
        }
        internal static IList<ILeveledBookmark> GetAllPages(IList<ILeveledBookmark> bookmarks, int documentEndPage)
        {
            if (bookmarks == null)
                return new List<ILeveledBookmark>();

            List<ILeveledBookmark> withEndPages = new List<ILeveledBookmark>();

            for (int i = 0; i < bookmarks.Count; i++)
            {
                ILeveledBookmark current = bookmarks[i];
                int endPage = documentEndPage;

                // End page is the end of document, unless a bookmark with lower or equal level
                // is found AFTER current bookmark
                for (int j = i + 1; j < bookmarks.Count; j++)
                {
                    ILeveledBookmark comparison = bookmarks[j];
                    if (comparison.Level <= current.Level &&
                        comparison.Pages[0] > current.Pages[0])
                    {
                        endPage = comparison.Pages[0] - 1;
                        break;
                    }
                }
                List<int> allPages = new List<int>();
                for (int k = current.Pages[0]; k <= endPage; k++)
                {
                    allPages.Add(k);
                }
                withEndPages.Add(new LeveledBookmark(current.Level, current.Title, allPages));
            }

            return withEndPages;
        }
    }
}
