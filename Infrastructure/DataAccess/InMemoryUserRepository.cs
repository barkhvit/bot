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
        private List<ToDoUser> _toDoUsers = new List<ToDoUser>();
        public async Task Add(ToDoUser user, CancellationToken cancellationToken)
        {
            _toDoUsers.Add(user);
            await Task.CompletedTask;
        }

        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            return _toDoUsers.FirstOrDefault(u => u.UserId == userId);
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken)
        {
            return _toDoUsers.FirstOrDefault(u => u.telegramUserId == telegramUserId);
        }
    }
}
