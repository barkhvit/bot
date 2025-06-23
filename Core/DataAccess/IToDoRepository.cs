using Bot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.DataAccess
{
    public interface IToDoRepository
    {
        Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken);
        //Возвращает ToDoItem для UserId со статусом Active
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken);
        Task Add(ToDoItem item, CancellationToken cancellationToken);
        Task Update(ToDoItem item, CancellationToken cancellationToken);
        Task Delete(Guid id, CancellationToken cancellationToken);
        //Проверяет есть ли задача с таким именем у пользователя
        Task<bool> ExistsByName(Guid userId, string name, CancellationToken cancellationToken);
        //Возвращает количество активных задач у пользователя
        Task<int> CountActive(Guid userId, CancellationToken cancellationToken);
        Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken);

        Task<IReadOnlyList<ToDoItem>> GetAll(CancellationToken ct);

    }
}
