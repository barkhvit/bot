using Bot.Core.DataAccess;
using Bot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Infrastructure.DataAccess
{
    public class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _toDoItems = new();

        //возвращает активные задачи для пользователя
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
        {
            return _toDoItems.Where(i => i.User.UserId == userId && i.State == ToDoItemState.Active).ToList();
        }

        //возвращает все задачи для пользователя
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
        {
            return _toDoItems.Where(i => i.User.UserId == userId).ToList();
        }

        //добавить задач
        public async Task Add(ToDoItem item, CancellationToken cancellationToken)
        {
            _toDoItems.Add(item);
        }

        //обновить задачу, найти по id и заменить на новую
        public async Task Update(ToDoItem item, CancellationToken cancellationToken)
        {
            var result = _toDoItems.FirstOrDefault(i => i.Id == item.Id);
            if(result != null)
            {
                _toDoItems.Remove(result);
                _toDoItems.Add(item);
            }
        }

        //кол-во активных задач
        public async Task<int> CountActive(Guid userId, CancellationToken cancellationToken)
        {
            var result = await GetActiveByUserId(userId, cancellationToken);
            return result.Count;
        }

        //удаляет задачу по id
        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            var item = _toDoItems.FirstOrDefault(i => i.Id == id);
            if (item != null)
                _toDoItems.Remove(item);
        }

        //проверяет есть такая задача или нет
        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken cancellationToken)
        {
            var item = _toDoItems.Where(i => i.User.UserId == userId && i.Name == name);
            return item != null;
        }

        //поиск по какому-то условию
        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            var result = await GetAllByUserId(userId, cancellationToken);
            return result.Where(predicate).ToList();


        }
    }
}
