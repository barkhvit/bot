using Bot.Core.DataAccess;
using Bot.Core.Entities;
using Bot.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.Core.Services
{
    public class ToDoListService : IToDoListService
    {
        private IToDoListRepository _toDoListRepository;
        public ToDoListService(IToDoListRepository toDoListRepository)
        {
            _toDoListRepository = toDoListRepository;
        }
        public async Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct)
        {
            //перед добавлением добавить проверки:
            //имя листа не больше 10 символов
            //название списка должно быть уникально в рамках одного ToDoUser
            if (name.Length > 10) throw new AddTodoListException("Имя списка не может быть больше 10 символов");
            if (await _toDoListRepository.ExistsByName(user.UserId, name, ct))
            {
                var toDoLists = await _toDoListRepository.GetByUserId(user.UserId, ct);
                return toDoLists.FirstOrDefault(l => l.Name == name);
            }
            else
            {
                ToDoList toDoList = new()
                {
                    User = user,
                    Id = Guid.NewGuid(),
                    Name = name,
                    CreatedAt = DateTime.UtcNow
                };
                await _toDoListRepository.Add(toDoList, ct);
                return toDoList;
            }
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            await _toDoListRepository.Delete(id, ct);
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            return await _toDoListRepository.Get(id, ct);
        }

        public async Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct)
        {
            return await _toDoListRepository.GetByUserId(userId, ct);
        }
    }
}
