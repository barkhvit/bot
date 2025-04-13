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
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _toDoItems.Where(i => i.User.UserId == userId && i.State == ToDoItemState.Active).ToList();
        }

        //возвращает все задачи для пользователя
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _toDoItems.Where(i => i.User.UserId == userId).ToList();
        }

        //добавить задач
        public void Add(ToDoItem item)
        {
            _toDoItems.Add(item);
        }

        //обновить задачу, найти по id и заменить на новую
        public void Update(ToDoItem item)
        {
            var result = _toDoItems.FirstOrDefault(i => i.Id == item.Id);
            if(result != null)
            {
                _toDoItems.Remove(result);
                _toDoItems.Add(item);
            }
        }

        //кол-во активных задач
        public int CountActive(Guid userId)
        {
            return GetActiveByUserId(userId).Count;
        }

        //удаляет задачу по id
        public void Delete(Guid id)
        {
            var item = _toDoItems.FirstOrDefault(i => i.Id == id);
            if (item != null)
                _toDoItems.Remove(item);
        }

        //проверяет есть такая задача или нет
        public bool ExistsByName(Guid userId, string name)
        {
            var item = _toDoItems.Where(i => i.User.UserId == userId && i.Name == name);
            return item != null;
        }

        //поиск по какому-то условию
        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {
            return GetAllByUserId(userId).Where(predicate).ToList();
        }
    }
}
