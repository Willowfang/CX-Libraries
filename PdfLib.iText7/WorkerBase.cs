using CX.Common.Base;
using CX.LoggingLib;
using CX.PdfLib.Common;
using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CX.PdfLib.iText7
{
    public abstract class WorkerBase<TDerived> : CommonWorkerBase<TDerived>
    {
        protected List<PdfDocument> OpenedDocuments = new();

        public WorkerBase(ILogbook logbook) : base(logbook) { }

        protected override void PrepareCleanUp()
        {
            OpenedDocuments = new();
            base.PrepareCleanUp();
        }

        protected override void CleanUp()
        {
            foreach (PdfDocument doc in OpenedDocuments)
            {
                if (doc.IsClosed() == false)
                {
                    doc.Close();
                }
            }

            base.CleanUp();
        }
    }
}
