using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Bot.Core.Entities
{
    public class ToDoItem
    {
        public Guid Id { get; set; }
        public ToDoUser User { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }
        public DateTime Deadline { get; set; }
        public ToDoList? ToDoList { get; set; }

        [JsonConstructor]
        public ToDoItem(Guid id, ToDoUser user, string name, DateTime createdAt, ToDoItemState state, DateTime deadLine, ToDoList? toDoList)
        {
            Id = id;
            User = user;
            Name = name;
            CreatedAt = createdAt;
            State = state;
            Deadline = deadLine;
            ToDoList = toDoList;
        }
        
        // Основной конструктор для создания новых задач
        public ToDoItem(ToDoUser user, string name, DateTime deadLine, ToDoList? toDoList)
            : this(Guid.NewGuid(), user, name, DateTime.UtcNow, ToDoItemState.Active, deadLine, toDoList)
        {
            // Всё уже сделано в [JsonConstructor]
        }

        public ToDoItem() { }

    }
}
