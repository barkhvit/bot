using Bot.Core.DataAccess;
using Bot.Core.Entities;
using Bot.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bot.Infrastructure.DataAccess
{
    public class FileToDoRepository : IToDoRepository
    {
        private readonly string _storageDirectory;
        private Dictionary<Guid, Guid> _items;
        private readonly string index = "index.json";

        public FileToDoRepository(string storageDirectory)
        {
            if (string.IsNullOrWhiteSpace(storageDirectory))
                throw new ArgumentException("Имя директории не может быть пустым.", nameof(storageDirectory));

            _storageDirectory = storageDirectory;
            Directory.CreateDirectory(_storageDirectory);

            //создаем и наполняем "index.json"
            _items = RebuildIndex();
            SaveIndex(_items);
        }

        public async Task Add(ToDoItem item, CancellationToken cancellationToken)
        {
            string directoryPath = Path.Combine(_storageDirectory, item.User.UserId.ToString());
            Directory.CreateDirectory(directoryPath);
            string filePath = Path.Combine(directoryPath, $"{item.Id}.json");
            string json = JsonSerializer.Serialize(item, new JsonSerializerOptions { WriteIndented = true , Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            await File.WriteAllTextAsync(filePath, json, cancellationToken);

            //обновляем index.json
            var items = await RebuildIndexAsync();
            if (items != null) _items = items;
            await SaveIndexAsync(_items);
        }

        public async Task<int> CountActive(Guid userId, CancellationToken cancellationToken)
        {
            var activeItems = await GetActiveByUserId(userId, cancellationToken);
            return activeItems.Count();
        }

        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            if(!_items.TryGetValue(id,out var userId))
            {
                throw new NoтExistentTaskException();
            }

            string fileName = $"{id}.json";
            string filePath = Path.Combine(_storageDirectory, userId.ToString(), fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _items.Remove(id);
                await SaveIndexAsync(_items);
            }
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken cancellationToken)
        {
            string directoryPath = Path.Combine(_storageDirectory, userId.ToString());
            Directory.CreateDirectory(directoryPath);
            var allItems = await GetAllByUserId(userId, cancellationToken);
            var result = allItems.Where(i => i.Id == userId && i.Name == name).ToList();
            return result != null;
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            var allItems = await GetAllByUserId(userId, cancellationToken);
            return allItems.Where(predicate).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
        {
            var items = await GetAllByUserId(userId, cancellationToken);
            return items.Where(i => i.State == ToDoItemState.Active).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
        {
            List<ToDoItem> items = new();
            string directoryPath = Path.Combine(_storageDirectory, userId.ToString());
            foreach(string file in Directory.GetFiles(directoryPath))
            {
                string json = await File.ReadAllTextAsync(file, cancellationToken);
                var item = JsonSerializer.Deserialize<ToDoItem>(json);

                if (item != null)
                {
                    items.Add(item);
                }
            }
            return items.AsReadOnly();
        }
        public async Task Update(ToDoItem item, CancellationToken cancellationToken)
        {
            var allItems = await GetAllByUserId(item.User.UserId,cancellationToken);
            if (allItems != null)
            {
                await Delete(item.Id, cancellationToken);
                await Add(item, cancellationToken);
            }
        }
        //--------------------------------------------------------------------------------------------------
        //сканирование всех папок и создание словаря
        public Dictionary<Guid, Guid> RebuildIndex()
        {
            Dictionary<Guid, Guid> index = new Dictionary<Guid, Guid>();
            foreach (string dir in Directory.GetDirectories(_storageDirectory))
            {
                string userId = Path.GetFileName(dir);
                if(Guid.TryParse(userId,out var userIdGuid))
                {
                    foreach (string fileDir in Directory.GetFiles(dir))
                    {
                        string itemId = Path.GetFileNameWithoutExtension(fileDir);
                        if(Guid.TryParse(itemId,out var itemIdGuid))
                        {
                            index[itemIdGuid] = userIdGuid;
                        }
                    }
                }
            }
            return index; 
        }

        //сканирование всех папок и создание словаря асинхронно
        public async Task<Dictionary<Guid, Guid>> RebuildIndexAsync()
        {
            Dictionary<Guid, Guid> index = new Dictionary<Guid, Guid>();
            foreach (string dir in Directory.GetDirectories(_storageDirectory))
            {
                string userId = Path.GetFileName(dir);
                if (Guid.TryParse(userId, out var userIdGuid))
                {
                    foreach (string fileDir in Directory.GetFiles(dir))
                    {
                        string itemId = Path.GetFileNameWithoutExtension(fileDir);
                        if (Guid.TryParse(itemId, out var itemIdGuid))
                        {
                            index[itemIdGuid] = userIdGuid;
                        }
                    }
                }
            }
            return await Task.FromResult(index);
        }
        //--------------------------------------------------------------------------------------------------
        //обновляем "index.json"
        public void SaveIndex(Dictionary<Guid, Guid> items)
        {
            string json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
            string filePath = Path.Combine(_storageDirectory, index);
            File.WriteAllText(filePath, json);
        }
        public async Task SaveIndexAsync(Dictionary<Guid, Guid> items)
        {
            string json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
            string filePath = Path.Combine(_storageDirectory, index);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAll(CancellationToken ct)
        {
            List<ToDoItem> toDoItems = new();
            foreach(var dir in Directory.GetDirectories(_storageDirectory))
            {
                foreach(var file in Directory.GetFiles(dir))
                {
                    string json = await File.ReadAllTextAsync(file, ct);
                    var item = JsonSerializer.Deserialize<ToDoItem>(json);
                    if(item!=null) toDoItems.Add(item);
                }
            }
            return toDoItems.AsReadOnly();
        }
    }
}
