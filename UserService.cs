using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public class UserService : IUserService
    {
        //хранилище с пользователями
        List<User> _users = new();

        //возвращает null если пользователя нет в хранилище
        public User? GetUser(long telegramUserId)
        {
            foreach(User u in _users)
            {
                if (u.TelegramUserId == telegramUserId)
                    return u;
            }
            return null;
        }

        //регистрация пользователя
        public User RegisterUser(long telegramUserId, string telegramUserName)
        {
            //проверяем зарегистрирован пользователь или нет? Возвращаем пользователя из Хранилища, если зарегистрирован
            if (GetUser(telegramUserId) == null)
            {
                //Если не зарегистрирован, то создаем нового, добавляем в Хранилище и возвращаем его
                User user = new User()
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
