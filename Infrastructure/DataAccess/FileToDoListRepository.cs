using Bot.Core.DataAccess;
using Bot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bot.Infrastructure.DataAccess
{
    public class FileToDoListRepository : IToDoListRepository
    {
        private string _storage;
        public FileToDoListRepository(string storage)
        {
            if (String.IsNullOrWhiteSpace(storage))
                throw new ArgumentException("Директория не может быть пустая");
            _storage = storage;
            try
            {
                if (!Directory.Exists(_storage)) Directory.CreateDirectory(_storage);
            }
            catch (Exception)
            {
                Console.WriteLine("Ошибка при создании хранилища для списков задач");
                throw; // Перебрасываем исключение для остановки работы
            }
        }
        public async Task Add(ToDoList list, CancellationToken ct)
        {
            string dirPath = Path.Combine(_storage, list.User.telegramUserId.ToString());
            Directory.CreateDirectory(dirPath);
            string filePath = Path.Combine(dirPath, $"{list.Id.ToString()}.json");
            string json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json, ct);
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            var list = await Get(id, ct);
            if (list != null)
            {
                string filePath = Path.Combine(_storage, list.User.telegramUserId.ToString(), $"{id}.json");
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            var listsByUserId = await GetByUserId(userId, ct);
            return listsByUserId.Any(l => l.Name == name);
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            var lists = await GetAll(ct);
            return lists.FirstOrDefault(l => l.Id == id);
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            var lists = await GetAll(ct);
            return lists.Where(l => l.User.UserId == userId).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<ToDoList>> GetAll(CancellationToken ct)
        {
            List<ToDoList> lists = new List<ToDoList>();
            foreach (string dir in Directory.GetDirectories(_storage))
            {
                foreach (string file in Directory.GetFiles(dir))
                {
                    string json = await File.ReadAllTextAsync(file, ct);
                    ToDoList? toDoList = JsonSerializer.Deserialize<ToDoList>(json);
                    if (toDoList != null) lists.Add(toDoList);
                }
            }
            return lists.AsReadOnly();
        }
    }
}
