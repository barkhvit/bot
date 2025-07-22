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
    public class SqlToDoRepository : IToDoRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlToDoRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var items = await dbContext.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .Where(i => i.UserId == userId)
                .ToListAsync(cancellationToken);

            return items.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var items = await dbContext.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .Where(i => i.UserId == userId && i.State == (int)ToDoItemState.Active)
                .ToListAsync(cancellationToken);

            return items.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task Add(ToDoItem item, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();
            await dbContext.InsertAsync(ModelMapper.MapToModel(item), token: cancellationToken);
        }

        public async Task Update(ToDoItem item, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();
            await dbContext.UpdateAsync(ModelMapper.MapToModel(item), token: cancellationToken);
        }

        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();
            await dbContext.ToDoItems
                .Where(i => i.Id == id)
                .DeleteAsync(cancellationToken);
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();
            return await dbContext.ToDoItems
                .AnyAsync(i => i.UserId == userId && i.Name == name, cancellationToken);
        }

        public async Task<int> CountActive(Guid userId, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();
            return await dbContext.ToDoItems
                .CountAsync(i => i.UserId == userId && i.State == (int)ToDoItemState.Active, cancellationToken);
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();

            var items = await dbContext.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .Where(i => i.UserId == userId)
                .ToListAsync(cancellationToken);

            return items.Select(ModelMapper.MapFromModel).Where(predicate).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAll(CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var items = await dbContext.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .ToListAsync(ct);

            return items.Select(ModelMapper.MapFromModel).ToList();
        }
    }
}
