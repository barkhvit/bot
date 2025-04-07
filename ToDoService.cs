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
        const int TaskLimit = 30;//лимит по длине задачи
        const int ToDoItemsLimit = 5; //лимит по кол-ву задач на пользователя


        List<ToDoItem> _toDoItems = new List<ToDoItem>();
        public List<ToDoItem> GetToDoItems()
        {
            return _toDoItems;
        }

        //добавляет задачу и возвращает ее
        public ToDoItem Add(User user, string[] text)
        {
            if (text.Length == 1 || text[1] == "") throw new IncorrectTaskException();//проверяем, что задача не пустая строка
            if (!IsNameNotRepeats(text[1], user.UserId)) throw new DuplicateTaskException(text[1]); // проверяем задачу на дубликат для этого пользователя
            if (text[1].Length > TaskLimit) throw new TaskLengthLimitException(text[1].Length, TaskLimit); //проверяем, что длина задачи не превышает допустимое значение TaskLimit
            int i = text[1].Replace(" ", "").Length; if (i == 0) throw new IncorrectTaskException();//проверяем что строка не состоит только из пробелов

            ToDoItem toDoItem = new(user, text[1]);
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
                if (toDoItem.Id == id)
                {
                    _toDoItems.RemoveAt(i);
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
        private bool IsNameNotRepeats(string name,Guid userId)
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
