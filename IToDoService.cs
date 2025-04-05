using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public interface IToDoService
    {
        //Возвращает ToDoItem для UserId со статусом Active
        IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId);
        ToDoItem Add(User user, string name);
        void MarkCompleted(Guid id, out bool isComplete);
        void Delete(Guid id, out bool isDelete);

        //перебирает по именам активные задачи и проверяет есть такая задача или нет у данного пользователя
        bool IsNameNotRepeats(string name, Guid userId);

        //возвращает список задач
        List<ToDoItem> GetToDoItems();

        //Возвращает все ToDoItem для UserId
        IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId);
    }
}
