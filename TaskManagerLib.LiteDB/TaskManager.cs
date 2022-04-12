using LiteDB;
using LiteDB.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.TaskManagerLib.LiteDB
{
    public class TaskManager : ITaskManager
    {
        string connectionString;

        public TaskManager(string databasePath)
        {
            connectionString = $"Filename={databasePath};Connection=shared";
        }

        public async Task<bool> AddOrUpdate(TaskItem item)
        {
            using (var db = new LiteDatabaseAsync(connectionString))
            {
                var collection = db.GetCollection<TaskItem>();
                return await collection.UpsertAsync(item);
            }
        }

        public async Task<bool> Remove(TaskItem item)
        {
            using (var db = new LiteDatabaseAsync(connectionString))
            {
                var collection = db.GetCollection<TaskItem>();
                return await collection.DeleteAsync(item.Id);
            }
        }

        public async Task<IList<TaskItem>> GetAll()
        {
            using (var db = new LiteDatabaseAsync(connectionString))
            {
                var collection = db.GetCollection<TaskItem>();
                return await collection.Query().ToListAsync();
            }
        }
    }
}
