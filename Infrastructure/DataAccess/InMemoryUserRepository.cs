using Bot.Core.DataAccess;
using Bot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Infrastructure.DataAccess
{
    class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> _toDoUsers = new List<ToDoUser>();
        public async Task Add(ToDoUser user, CancellationToken cancellationToken)
        {
            _toDoUsers.Add(user);
            await Task.CompletedTask;
        }

        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            var user = _toDoUsers.FirstOrDefault(u => u.UserId == userId);
            return await Task.FromResult(user);
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_toDoUsers.FirstOrDefault(u => u.telegramUserId == telegramUserId));
        }
    }
}
