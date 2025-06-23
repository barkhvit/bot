using Bot.Core.DataAccess;
using Bot.Core.Entities;
using Bot.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Services
{
    public class ToDoService : IToDoService
    {
        private int TaskLimit = 30;//лимит по длине задачи
        private readonly IToDoRepository _toDoRepository;

        //КОНСТРУКТОР
        public ToDoService(int taskLimit, IToDoRepository toDoRepository)
        {
            TaskLimit = taskLimit;
            _toDoRepository = toDoRepository;
        }


        public async Task<ToDoItem> Add(ToDoUser user, string text, DateTime deadLine, ToDoList? toDoList, CancellationToken cancellationToken)
        {
            ToDoItem toDoItem = new(user, text,deadLine, toDoList);
            await _toDoRepository.Add(toDoItem, cancellationToken);
            return toDoItem;
        }

        //удаление задачи по Id
        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            await _toDoRepository.Delete(id, cancellationToken);      
        }

        //возвращает список задач по UserId, только со статусом Active(LINQ)
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
        {
            return await _toDoRepository.Find(userId, item => item.State == ToDoItemState.Active, cancellationToken);
        }

        
        //переводит статус задачи в исполнено
        public async Task MarkCompleted(Guid id, CancellationToken ct)
        {
            bool isCompleted = false;
            var items = await _toDoRepository.GetAll(ct);
            var toDoItem = items.FirstOrDefault(i => i.Id == id);
            if (toDoItem != null)
            {
                toDoItem.State = ToDoItemState.Completed;
                toDoItem.StateChangedAt = DateTime.UtcNow;
                await _toDoRepository.Delete(id, ct);
                await _toDoRepository.Add(toDoItem, ct);
                isCompleted = true;
            }
            if (!isCompleted) throw new NoтExistentTaskException();
        }

        //перебирает по именам активные задачи и проверяет есть такая задача или нет у данного пользователя
        private async Task<bool> IsNameNotRepeats(string name,Guid userId, CancellationToken cancellationToken)
        {
            return await _toDoRepository.ExistsByName(userId, name, cancellationToken);
        }


        //возвращает все задачи для пользователя
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
        {
            return await _toDoRepository.GetAllByUserId(userId, cancellationToken);
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken cancellationToken)
        {
            return await _toDoRepository.Find(user.UserId, item => item.Name.StartsWith(namePrefix), cancellationToken);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct)
        {
            var toDoItems = await _toDoRepository.GetAllByUserId(userId, ct);
            if (listId == null)
            {
                return toDoItems.Where(i => i.ToDoList == null).ToList().AsReadOnly();
            }
            return toDoItems.Where(i => i.ToDoList != null && i.ToDoList.Id == listId).ToList().AsReadOnly();
        }

        public async Task<ToDoItem?> Get(Guid toDoItemId, CancellationToken ct)
        {
            var items = await _toDoRepository.GetAll(ct);
            return items.FirstOrDefault(i => i.Id == toDoItemId);
        }
    }
}
