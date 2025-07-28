using Bot.Core.Entities;
using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.DataAccess.Models
{
    [Table("todoitem")]
    public class ToDoItemModel
    {
        [PrimaryKey]
        [Column("id")] public Guid Id { get; set; }

        [Column("name", Length = 100)] public string Name { get; set; }

        [Column("userid")] public Guid UserId { get; set; }

        [Column("listid")] public Guid? ListId { get; set; }

        [Column("createdat")] public DateTime CreatedAt { get; set; }

        [Column("state")] public int State { get; set; }

        [Column("statechangedat")] public DateTime? StateChangedAt { get; set; }

        [Column("deadline")] public DateTime Deadline { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.UserId))]
        public ToDoUserModel User { get; set; }

        [Association(ThisKey = nameof(ListId), OtherKey = nameof(ToDoListModel.Id))]
        public ToDoListModel List { get; set; }
    }
}
