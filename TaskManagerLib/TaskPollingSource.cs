using WF.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.TaskManagerLib
{
    public class TaskPollingSource : IdClass
    {
        public string DirectoryPath { get; }
        public string Definition { get; }
        public bool PollForFiles { get; }
        public bool PollForDirectories { get; }

        public TaskPollingSource(Guid id, DateTime creationDate, 
            string directoryPath, string definition,
            bool pollForFiles, bool pollForDirectories) : base(id, creationDate)
        {
            DirectoryPath = directoryPath;
            Definition = definition;
            PollForFiles = pollForFiles;
            PollForDirectories = pollForDirectories;
        }

        public TaskPollingSource CreateNew(string directoryPath,
            string definition, bool pollForFiles, bool pollForDirectories)
        {
            return new TaskPollingSource(Guid.NewGuid(), DateTime.Now, directoryPath, definition, 
                pollForFiles, pollForDirectories);
        }
    }
}
