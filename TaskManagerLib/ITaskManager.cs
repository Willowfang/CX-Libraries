using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.TaskManagerLib
{
    public interface ITaskManager
    {
        public Task<bool> AddOrUpdate(TaskItem item);
        public Task<bool> Remove(TaskItem item);
        public Task<IList<TaskItem>> GetAll();
    }
}
