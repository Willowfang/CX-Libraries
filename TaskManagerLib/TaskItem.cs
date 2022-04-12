using CX.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.TaskManagerLib
{
    public class TaskItem : IdClass
    {
        public string? Name { get; set; }
        public int Priority { get; set; }
        public TaskStatusItem? Status { get; set; }
        public TaskDeadlineItem? Deadline { get; set; }
        public List<string>? Notes { get; set; }

    }
}
