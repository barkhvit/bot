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
        public Guid Id { get; private set; }
        public ToDoUser User { get; private set; }
        public string Name { get; private set; }
        public DateTime CreatedAt { get; private set; }
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
        public ToDoItem(ToDoUser user,string name, DateTime deadLine, ToDoList? toDoList)
        {
            Id = Guid.NewGuid();
            User = user;
            Name = name;
            CreatedAt = DateTime.UtcNow;
            State = ToDoItemState.Active;
            Deadline = deadLine;
            ToDoList = toDoList;
        }

    }
}
