using Bot.Core.DataAccess;
using Bot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.Infrastructure.DataAccess
{
    class FileUserRepository : IUserRepository
    {
        private readonly string _storageUsers;

        public FileUserRepository(string storageUsers)
        {
            if (String.IsNullOrWhiteSpace(storageUsers))
                throw new ArgumentException("Директория не может быть пустая");
            _storageUsers = storageUsers;
            try
            {
                if (!Directory.Exists(_storageUsers)) Directory.CreateDirectory(_storageUsers);
            }
            catch(Exception)
            {
                Console.WriteLine("Ошибка при создании директории пользователей.");
                throw; // Перебрасываем исключение для остановки работы
            }
        }

        public async Task Add(ToDoUser user, CancellationToken cancellationToken)
        {
            string directoryPath = Path.Combine(_storageUsers);
            string filePath = Path.Combine(directoryPath, $"{user.UserId}.json");
            string json = JsonSerializer.Serialize<ToDoUser>(user, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json, cancellationToken);
        }

        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            var users = await GetAllUsers(cancellationToken);
            return users.FirstOrDefault(u => u.UserId == userId);
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken)
        {
            var users = await GetAllUsers(cancellationToken);
            return users.FirstOrDefault(u => u.telegramUserId == telegramUserId);
        }

        public async Task<IReadOnlyList<ToDoUser>> GetAllUsers(CancellationToken cancellationToken)
        {
            List<ToDoUser> users = new();
            foreach(string file in Directory.GetFiles(_storageUsers))
            {
                string json = await File.ReadAllTextAsync(file, cancellationToken);
                var user = JsonSerializer.Deserialize<ToDoUser>(json);
                if (user != null)
                    users.Add(user);
            }
            return users.AsReadOnly();
        }
    }
}
