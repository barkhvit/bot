using Otus.ToDoList.ConsoleBot.Types;
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
        public void Delete(Guid id, out bool isDelete)
        {
            isDelete = false;
            int i = 0;
            foreach(ToDoItem toDoItem in _toDoItems)
            {
                if (toDoItem.Id == id) _toDoItems.RemoveAt(i);
                {
                    isDelete = true;
                    break;
                }
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
        public void MarkCompleted(Guid id, out bool isComplete)
        {
            isComplete = false;
            foreach (ToDoItem toDoItem in _toDoItems)
            {
                if (toDoItem.Id == id)
                {
                    toDoItem.State = ToDoItemState.Completed;
                    toDoItem.StateChangedAt = DateTime.UtcNow;
                    isComplete = true;
                }
                    
            }
        }

        //перебирает по именам активные задачи и проверяет есть такая задача или нет у данного пользователя
        public bool IsNameNotRepeats(string name,Guid userId)
        {
            bool answer = true;
            foreach(ToDoItem Item in GetActiveByUserId(userId))
            {
                if (Item.Name == name) answer = false;
            }
            return answer;
        }

        //возвращает все задачи для пользователя
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            List<ToDoItem> toDoItemsActive = new List<ToDoItem>();
            foreach (ToDoItem toDoItem in _toDoItems)
            {
                if (toDoItem.User.UserId == userId)
                    toDoItemsActive.Add(toDoItem);
            }
            return toDoItemsActive;
        }
    }
}
