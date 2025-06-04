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


        //добавляет задачу и возвращает ее
        public async Task<ToDoItem> Add(ToDoUser user, string[] text, CancellationToken cancellationToken)
        {
            if (user == null) throw new UserIsNotRegistratedException();//если пользователь не зарегистрирован
            if (text.Length == 1 || text[1] == "") throw new IncorrectTaskException();//проверяем, что задача не пустая строка
            if (!await IsNameNotRepeats(text[1], user.UserId, cancellationToken)) throw new DuplicateTaskException(text[1]); // проверяем задачу на дубликат для этого пользователя
            if (text[1].Length > TaskLimit) throw new TaskLengthLimitException(text[1].Length, TaskLimit); //проверяем, что длина задачи не превышает допустимое значение TaskLimit
            int i = text[1].Replace(" ", "").Length; if (i == 0) throw new IncorrectTaskException();//проверяем что строка не состоит только из пробелов

            ToDoItem toDoItem = new(user, text[1]);
            await _toDoRepository.Add(toDoItem, cancellationToken);
            return toDoItem;
        }

        public async Task<ToDoItem> Add(ToDoUser user, string text, CancellationToken cancellationToken)
        {
            ToDoItem toDoItem = new(user, text);
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
        public async Task MarkCompleted(Guid id, Guid userId, CancellationToken cancellationToken)
        {
            bool isDelete = false;
            var items = await _toDoRepository.GetActiveByUserId(userId, cancellationToken);
            foreach (ToDoItem toDoItem in items)
            {
                if (toDoItem.Id == id)
                {
                    toDoItem.State = ToDoItemState.Completed;
                    toDoItem.StateChangedAt = DateTime.UtcNow;
                    isDelete = true;
                }  
            }
            if (!isDelete) throw new NoтExistentTaskException();
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
    }
}
