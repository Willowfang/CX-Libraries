using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.PdfLib.Common
{
    public enum ProgressPhase
    {
        Unassigned,
        AddingBookmarks,
        AddingPageNumbers,
        Converting,
        Extracting,
        GettingBookmarks,
        Merging,
        Finished
    }

    /// <summary>
    /// Class for reporting progress of a task
    /// </summary>
    public class ProgressReport
    {
        public int Percentage { get; }
        public ProgressPhase CurrentPhase { get; }
        public string CurrentItem { get; }

        public ProgressReport(int percentage = 0, ProgressPhase currentPhase = ProgressPhase.Unassigned,
            string currentItem = null)
        {
            Percentage = percentage;
            CurrentPhase = currentPhase;
            CurrentItem = currentItem;
        }
    }
}
