using Bot.Core.DataAccess;
using Bot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public async Task<ToDoUser?> GetUser(long telegramUserId, CancellationToken cancellationToken)
        {
            return await _userRepository.GetUserByTelegramUserId(telegramUserId, cancellationToken);
        }

        //регистрация пользователя
        public async Task<ToDoUser> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken cancellationToken)
        {
            //проверяем зарегистрирован пользователь или нет? Возвращаем пользователя из Хранилища, если зарегистрирован
            if (await GetUser(telegramUserId, cancellationToken) == null)
            {
                //Если не зарегистрирован, то создаем нового, добавляем в Хранилище и возвращаем его
                ToDoUser user = new ToDoUser()
                {
                    UserId = Guid.NewGuid(),
                    telegramUserId = telegramUserId,
                    TelegramUserName = telegramUserName,
                    RegisteredAt = DateTime.UtcNow
                };
                await _userRepository.Add(user, cancellationToken);
                return user;
            }
            else
                return await GetUser(telegramUserId, cancellationToken);
        }
    }
}
