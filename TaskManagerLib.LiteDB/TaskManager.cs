using CX.LoggingLib;
using LiteDB;
using LiteDB.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.TaskManagerLib.LiteDB
{
    public class TaskManager : LoggingEnabled<TaskManager>, ITaskManager
    {
        public event EventHandler<TaskPollingEventArgs>? PollingEvent;

        private readonly string connectionString;
        private bool polling;
        private Task? pollingTask;

        public TaskManager(ITaskConnection connection, ILogbook logbook)
            : base(logbook)
        {
            connectionString = $"Filename={connection.TaskDataStorageLocation};Connection=shared";
        }



        public async Task Poll()
        {
            polling = true;
            pollingTask = DoPolling();

            logbook.Write($"TaskItem polling started.", LogLevel.Information);

            await pollingTask;
        }

        public void StopPolling()
        {
            polling = false;

            logbook.Write($"TaskItem polling stopped.", LogLevel.Information);
        }

        private async Task DoPolling()
        {
            while (polling)
            {
                List<TaskItem> foundItems = new List<TaskItem>();
                IList<TaskPollingSource> sources = await GetSources();
                bool hasChanges = false;

                foreach (TaskPollingSource source in sources)
                {
                    foundItems.AddRange(await Task.Run(() => GetItems(source)));
                }

                IList<TaskItem> knownItems = await GetAllTaskItems();

                foreach (TaskItem previous in knownItems)
                {
                    TaskItem? found = foundItems.FirstOrDefault(x => x.Path.FullName == previous.Path.FullName);

                    if (found == null)
                    {
                        await RemoveTaskItem(previous);
                        hasChanges = true;
                    }
                    else if (found.Source.DirectoryPath != previous.Source.DirectoryPath)
                    {
                        TaskItem updated = new TaskItem(previous.Id,
                            previous.CreationDate, previous.Name, previous.Path,
                            previous.Priority, previous.Status, previous.Deadline,
                            previous.Notes, true, found.Source);

                        await AddOrModifyTaskItem(updated);
                        hasChanges = true;
                    }
                }

                foreach (TaskItem found in foundItems)
                {
                    if (knownItems.Any(x => x.Name == found.Name) == false)
                    {
                        await AddOrModifyTaskItem(found);
                        hasChanges = true;
                    }
                }

                if (hasChanges)
                {
                    TaskPollingEventArgs args = new TaskPollingEventArgs(await GetAllTaskItems());
                    OnHasChanges(args);
                }

                await Task.Delay(5000);
            }
        }

        private void OnHasChanges(TaskPollingEventArgs e)
        {
            EventHandler<TaskPollingEventArgs> handler = PollingEvent;
            handler?.Invoke(this, e);
        }

        private IList<TaskItem> GetItems(TaskPollingSource source)
        {
            List<TaskItem> items = new List<TaskItem>();

            if (source.PollForDirectories)
            {
                foreach (string path in Directory.GetDirectories(source.DirectoryPath))
                {
                    items.Add(TaskItem.Create(new DirectoryInfo(path), source));
                }
            }
            if (source.PollForFiles)
            {
                foreach (string path in Directory.GetFiles(source.DirectoryPath))
                {
                    items.Add(TaskItem.Create(new FileInfo(path), source));
                }
            }

            return items;
        }

        public async Task<bool> AddOrModifySource(TaskPollingSource source)
        {
            using (var db = new LiteDatabaseAsync(connectionString))
            {
                var collection = db.GetCollection<TaskPollingSource>();
                return await collection.UpsertAsync(source);
            }
        }

        public async Task<bool> RemoveSource(TaskPollingSource source)
        {
            using (var db = new LiteDatabaseAsync(connectionString))
            {
                var collection = db.GetCollection<TaskPollingSource>();
                return await collection.DeleteAsync(source.Id);
            }
        }

        private async Task<IList<TaskPollingSource>> GetSources()
        {
            using (var db = new LiteDatabaseAsync(connectionString))
            {
                var collection = db.GetCollection<TaskPollingSource>();
                return await collection.Query().ToListAsync();
            }
        }

        public async Task<bool> AddOrModifyTaskItem(TaskItem item)
        {
            using (var db = new LiteDatabaseAsync(connectionString))
            {
                var collection = db.GetCollection<TaskItem>();
                return await collection.UpsertAsync(item);
            }
        }

        public async Task<bool> RemoveTaskItem(TaskItem item)
        {
            using (var db = new LiteDatabaseAsync(connectionString))
            {
                var collection = db.GetCollection<TaskItem>();
                return await collection.DeleteAsync(item.Id);
            }
        }

        public async Task<IList<TaskItem>> GetAllTaskItems()
        {
            using (var db = new LiteDatabaseAsync(connectionString))
            {
                var collection = db.GetCollection<TaskItem>();
                return await collection.Query().ToListAsync();
            }
        }
    }
}
