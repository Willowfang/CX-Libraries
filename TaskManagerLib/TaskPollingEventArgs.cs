using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.TaskManagerLib
{
    public class TaskPollingEventArgs : EventArgs
    {
        public IList<TaskItem> TaskItems { get; }

        public TaskPollingEventArgs(IList<TaskItem> taskItems)
        {
            TaskItems = taskItems;
        }
    }
}
