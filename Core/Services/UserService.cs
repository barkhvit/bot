using Bot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Services
{
    public class UserService : IUserService
    {
        //хранилище с пользователями
        List<ToDoUser> _users = new();

        //возвращает null если пользователя нет в хранилище
        public ToDoUser? GetUser(long telegramUserId)
        {
            foreach(ToDoUser u in _users)
            {
                if (u.TelegramUserId == telegramUserId)
                    return u;
            }
            return null;
        }

        //регистрация пользователя
        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            //проверяем зарегистрирован пользователь или нет? Возвращаем пользователя из Хранилища, если зарегистрирован
            if (GetUser(telegramUserId) == null)
            {
                //Если не зарегистрирован, то создаем нового, добавляем в Хранилище и возвращаем его
                ToDoUser user = new ToDoUser()
                {
                    UserId = Guid.NewGuid(),
                    TelegramUserId = telegramUserId,
                    TelegramUserName = telegramUserName,
                    RegisteredAt = DateTime.UtcNow
                };
                _users.Add(user);
                return user;
            }
            else
                return GetUser(telegramUserId);
        }
    }
}
