using CX.PdfLib.Services;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using System.Collections.Generic;

namespace PdfLib.iText7
{
    public class Merger : IMerger
    {
        public IList<int> Merge(IList<string> sourcePaths, string outputPath)
        {
            PdfDocument product = new PdfDocument(new PdfWriter(outputPath));
            IList<int> pages = Merge(sourcePaths, product);
            product.Close();
            return pages;
        }
        internal static IList<int> Merge(IList<string> sourcePaths, PdfDocument product)
        {
            List<int> startPages = new List<int>();
            PdfMerger merger = new PdfMerger(product).SetCloseSourceDocuments(true);

            int currentStart = 1;
            foreach (string path in sourcePaths)
            {
                startPages.Add(currentStart);
                if (path != null)
                {
                    PdfDocument src = new PdfDocument(new PdfReader(path));
                    int srcPages = src.GetNumberOfPages();
                    merger.Merge(src, 1, srcPages);
                    currentStart += srcPages;
                }
            }

            return startPages;
        }
    }
}
