using WF.PdfLib.Services;
using WF.LoggingLib;
using WF.PdfLib.iText7.Extraction;

namespace WF.PdfLib.iText7
{
    /// <summary>
    /// Default implementation of <see cref="IExtractionService"/>.
    /// </summary>
    public class ExtractionService : LoggingEnabled<ExtractionService>, IExtractionService
    {
        private readonly IPdfAConvertService convertService;

        public ExtractionService(
            IPdfAConvertService convertService,
            ILogbook logbook) : base(logbook) 
        {
            this.convertService = convertService;
        }

        public IExtractionWorker CreateWorker()
        {
            return new ExtractionWorker(convertService, logbook.BaseLogbook);
        }
    }
}
