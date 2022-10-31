using WF.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.TaskManagerLib
{
    public class TaskItem : IdClass
    {
        public string Name { get; }
        public FileSystemInfo Path { get; }
        public int Priority { get; }
        public TaskStatusItem? Status { get; }
        public TaskDeadlineItem? Deadline { get; }
        public List<string> Notes { get; }
        public bool IsNew { get; }
        public TaskPollingSource Source { get; }

        public TaskItem(Guid id, DateTime creationDate, string name, 
            FileSystemInfo path,int priority, TaskStatusItem? status,
            TaskDeadlineItem? deadline, List<string> notes,
            bool isNew, TaskPollingSource source) : base(id, creationDate)
        {
            Name = name;
            Path = path;
            Priority = priority;
            Status = status;
            Deadline = deadline;
            Notes = notes;
            IsNew = isNew;
            Source = source;
        }

        public static TaskItem Create(FileSystemInfo sourcePath,
            TaskPollingSource source)
        {
            return new TaskItem(Guid.NewGuid(), DateTime.Now, sourcePath.Name, sourcePath,
                0, null, null, new List<string>(), true, source);
        }
    }
}
