using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.TaskManagerLib
{
    public interface ITaskManager
    {
        public event EventHandler<TaskPollingEventArgs> PollingEvent;

        public Task Poll();
        public void StopPolling();

        public Task<bool> AddOrModifySource(TaskPollingSource source);
        public Task<bool> RemoveSource(TaskPollingSource source);

        public Task<bool> AddOrModifyTaskItem(TaskItem item);
        public Task<bool> RemoveTaskItem(TaskItem item);
        public Task<IList<TaskItem>> GetAllTaskItems();
    }
}
