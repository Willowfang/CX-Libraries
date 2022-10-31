using WF.Common.Base;
using WF.LoggingLib;
using WF.PdfLib.Common;
using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WF.PdfLib.iText7
{
    /// <summary>
    /// Base class for task workers.
    /// </summary>
    /// <typeparam name="TDerived">Type of the derived class.</typeparam>
    public abstract class WorkerBase<TDerived> : CommonWorkerBase<TDerived>
    {
        /// <summary>
        /// <see cref="PdfDocument"/>s that were opened by this worker.
        /// </summary>
        protected List<PdfDocument> OpenedDocuments = new();

        /// <summary>
        /// Create a new instance of a worker.
        /// </summary>
        /// <param name="logbook">Logging service.</param>
        public WorkerBase(ILogbook logbook) : base(logbook) { }

        /// <summary>
        /// Prepare clean up procedures before initializing tasks.
        /// </summary>
        protected override void PrepareCleanUp()
        {
            OpenedDocuments = new();
            base.PrepareCleanUp();
        }

        /// <summary>
        /// Perform a clean up after completing or cancelling the task.
        /// </summary>
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
