using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.PdfLib.Common
{
    /// <summary>
    /// The phase the current operation is currently at.
    /// </summary>
    public enum ProgressPhase
    {
        /// <summary>
        /// Unknown or unspecifies phase (such as initializing the task).
        /// </summary>
        Unassigned,
        /// <summary>
        /// Currently adding bookmarks to a document.
        /// </summary>
        AddingBookmarks,
        /// <summary>
        /// Currently adding page numbers to a document.
        /// </summary>
        AddingPageNumbers,
        /// <summary>
        /// Currently converting a document to another file type.
        /// </summary>
        Converting,
        /// <summary>
        /// Currently extracting bookmarks from a document.
        /// </summary>
        Extracting,
        /// <summary>
        /// Currently retrieving bookmark information from a document.
        /// </summary>
        GettingBookmarks,
        /// <summary>
        /// Currently merging documents together.
        /// </summary>
        Merging,
        /// <summary>
        /// The user is currently choosing a destination path.
        /// </summary>
        ChoosingDestination,
        /// <summary>
        /// The task is done.
        /// </summary>
        Finished
    }

    /// <summary>
    /// Class for reporting progress on a task.
    /// </summary>
    public class ProgressReport
    {
        /// <summary>
        /// The percentage done of the current task.
        /// </summary>
        public int Percentage { get; }

        /// <summary>
        /// The <see cref="ProgressPhase"/> the task is currently at.
        /// </summary>
        public ProgressPhase CurrentPhase { get; }

        /// <summary>
        /// The item the task if currently processing.
        /// </summary>
        public string CurrentItem { get; }

        /// <summary>
        /// Create an instance for reporting progress from a task.
        /// </summary>
        /// <param name="percentage">Current completed percentage.</param>
        /// <param name="currentPhase">Current phase of the task.</param>
        /// <param name="currentItem">Item currently being processed.</param>
        public ProgressReport(int percentage = 0, ProgressPhase currentPhase = ProgressPhase.Unassigned,
            string currentItem = null)
        {
            Percentage = percentage;
            CurrentPhase = currentPhase;
            CurrentItem = currentItem;
        }
    }
}
