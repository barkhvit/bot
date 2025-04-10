using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public ToDoItem(ToDoUser user,string name)
        {
            Id = Guid.NewGuid();
            User = user;
            Name = name;
            CreatedAt = DateTime.UtcNow;
            State = ToDoItemState.Active;
        }
    }
}
