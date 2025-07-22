using Bot.Core.DataAccess;
using Bot.Core.Entities;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Infrastructure.DataAccess
{
    public class SqlUserRepository : IUserRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlUserRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var user = await dbContext.ToDoUsers
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

            return user != null ? ModelMapper.MapFromModel(user) : null;
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var user = await dbContext.ToDoUsers
                .FirstOrDefaultAsync(u => u.TelegramUserId == telegramUserId, cancellationToken);

            return user != null ? ModelMapper.MapFromModel(user) : null;
        }

        public async Task Add(ToDoUser user, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();
            await dbContext.InsertAsync(ModelMapper.MapToModel(user), token: cancellationToken);
        }
    }
}
