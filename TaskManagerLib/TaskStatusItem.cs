using WF.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.TaskManagerLib
{
    public class TaskStatusItem : IdClass
    {
        public string Name { get; }
        public string Color { get; }

        public TaskStatusItem(Guid id, DateTime creationDate, 
            string name, string color) : base(id, creationDate)
        {
            Name = name;
            Color = color;
        }

        public static TaskStatusItem CreateNew(string name, string color)
        {
            return new TaskStatusItem(Guid.NewGuid(), DateTime.Now, name, color);
        }
    }
}
