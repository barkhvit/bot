using Bot.Core.DataAccess;
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
        private readonly IUserRepository _userRepository;

        //КОНСТРУКТОР
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }


        //возвращает null если пользователя нет в хранилище
        public ToDoUser? GetUser(long telegramUserId)
        {
            return _userRepository.GetUserByTelegramUserId(telegramUserId);
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
                    telegramUserId = telegramUserId,
                    TelegramUserName = telegramUserName,
                    RegisteredAt = DateTime.UtcNow
                };
                _userRepository.Add(user);
                return user;
            }
            else
                return GetUser(telegramUserId);
        }
    }
}
