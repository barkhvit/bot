using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public class UserService : IUserService
    {
        List<User> _users = new();
        public User? GetUser(long telegramUserId)
        {
            foreach(User u in _users)
            {
                if (u.TelegramUserId == telegramUserId)
                    return u;
            }
            return null;
        }

        public User RegisterUser(long telegramUserId, string telegramUserName)
        {
            foreach (User u in _users)
            {
                if (u.TelegramUserId == telegramUserId)
                    return u;
            }
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
    }
}
