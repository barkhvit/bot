﻿using Bot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.DataAccess
{
    public interface IUserRepository
    {
        Task<ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken);
        Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken);
        Task Add(ToDoUser user, CancellationToken cancellationToken);
    }
}
