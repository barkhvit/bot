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
        IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId);//Возвращает список задач для UserId со статусом Active
        ToDoItem Add(ToDoUser user, string[] name);//добавляет задачу
        void MarkCompleted(Guid id, out bool isComplete);//делает задачу завершенной
        void Delete(Guid id, out bool isDelete);//удаление задачи
        //bool IsNameNotRepeats(string name, Guid userId);//перебирает по именам активные задачи и проверяет есть такая задача или нет у данного пользователя
        List<ToDoItem> GetToDoItems();//возвращает список задач
        IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId);//Возвращает все задачи для UserId

    }
}
