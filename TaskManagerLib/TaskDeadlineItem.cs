using CX.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.TaskManagerLib
{
    public class TaskDeadlineItem : IdClass
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public DateTime Deadline { get; set; }

        public TaskDeadlineItem(Guid id, DateTime creationDate, 
            string name, string color, DateTime deadline)
            : base(id, creationDate)
        {
            Name = name;
            Color = color;
            Deadline = deadline;
        }

        public static TaskDeadlineItem CreateNew(string name,
            string color, DateTime deadline)
        {
            return new TaskDeadlineItem(Guid.NewGuid(), DateTime.Now, name, color, deadline);
        }
    }
}
