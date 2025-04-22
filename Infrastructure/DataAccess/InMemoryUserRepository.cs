using Bot.Core.DataAccess;
using Bot.Core.Entities;
using Otus.ToDoList.ConsoleBot.Types;
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
        public void Add(ToDoUser user)
        {
            _toDoUsers.Add(user);
        }

        public ToDoUser? GetUser(Guid userId)
        {
            return (ToDoUser?)_toDoUsers.FirstOrDefault(u => u.UserId == userId);
        }

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
        {
            return (ToDoUser?)_toDoUsers.FirstOrDefault(u => u.telegramUserId == telegramUserId);
        }
    }
}
