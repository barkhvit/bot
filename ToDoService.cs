using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public class ToDoService : IToDoService
    {
        List<ToDoItem> _toDoItems = new List<ToDoItem>();
        public List<ToDoItem> GetToDoItems()
        {
            return _toDoItems;
        }

        //добавляет задачу и возвращает ее
        public ToDoItem Add(User user, string name)
        {
            ToDoItem toDoItem = new(user, name);
            _toDoItems.Add(toDoItem);
            return toDoItem;
        }

        //удаление задачи по Id
        public void Delete(Guid id)
        {
            int i = 0;
            foreach(ToDoItem toDoItem in _toDoItems)
            {
                if (toDoItem.Id == id) _toDoItems.RemoveAt(i);
                break;
                i++;
            }      
        }

        //возвращает список задач по UserId, только со статусом Active
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            List<ToDoItem> toDoItemsActive = new List<ToDoItem>();
            foreach(ToDoItem toDoItem in _toDoItems)
            {
                if(toDoItem.User.UserId == userId && toDoItem.State == ToDoItemState.Active)
                    toDoItemsActive.Add(toDoItem);
            }
            return toDoItemsActive;
        }

        //переводит статус задачи в исполнено
        public void MarkCompleted(Guid id)
        {
            foreach(ToDoItem toDoItem in _toDoItems)
            {
                if(toDoItem.Id == id)
                    toDoItem.State = ToDoItemState.Completed;
            }
        }
    }
}
