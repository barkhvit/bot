﻿using Bot.Core.DataAccess;
using Bot.Core.Entities;
using Bot.Core.Exceptions;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
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
        public ToDoItem Add(ToDoUser user, string[] text)
        {
            if (user == null) throw new UserIsNotRegistratedException();//если пользователь не зарегистрирован
            if (text.Length == 1 || text[1] == "") throw new IncorrectTaskException();//проверяем, что задача не пустая строка
            if (!IsNameNotRepeats(text[1], user.UserId)) throw new DuplicateTaskException(text[1]); // проверяем задачу на дубликат для этого пользователя
            if (text[1].Length > TaskLimit) throw new TaskLengthLimitException(text[1].Length, TaskLimit); //проверяем, что длина задачи не превышает допустимое значение TaskLimit
            int i = text[1].Replace(" ", "").Length; if (i == 0) throw new IncorrectTaskException();//проверяем что строка не состоит только из пробелов

            ToDoItem toDoItem = new(user, text[1]);
            _toDoRepository.Add(toDoItem);
            return toDoItem;
        }

        //удаление задачи по Id
        public void Delete(Guid id)
        {
            _toDoRepository.Delete(id);      
        }

        //возвращает список задач по UserId, только со статусом Active(LINQ)
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _toDoRepository.Find(userId, item => item.State == ToDoItemState.Active);
            //return _toDoRepository.GetActiveByUserId(userId);
        }

        
        //переводит статус задачи в исполнено
        public void MarkCompleted(Guid id, Guid userId)
        {
            bool isDelete = false;
            var items = _toDoRepository.GetActiveByUserId(userId);
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
        private bool IsNameNotRepeats(string name,Guid userId)
        {
            return _toDoRepository.ExistsByName(userId, name);
        }


        //возвращает все задачи для пользователя
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _toDoRepository.GetAllByUserId(userId);
        }

        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            return _toDoRepository.Find(user.UserId, item => item.Name.StartsWith(namePrefix));
        }
    }
}
