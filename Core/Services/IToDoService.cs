using Bot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Services
{
    public interface IToDoService
    {
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken);//Возвращает список задач для UserId со статусом Active
        Task<ToDoItem> Add(ToDoUser user, string text, DateTime deadLine, ToDoList? toDoList,CancellationToken cancellationToken);//добавляет задачу
        Task MarkCompleted(Guid id, Guid userId, CancellationToken cancellationToken);//делает задачу завершенной
        Task Delete(Guid id, CancellationToken cancellationToken);//удаление задачи
        Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken);//Возвращает все задачи для UserId
        Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken cancellationToken);
        Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct);
    }
}
