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
        public string? Name { get; set; }
        public string? Color { get; set; }
        public DateTime? DeadLine { get; set; }
    }
}
