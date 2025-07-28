using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.DataAccess.Models
{
    [Table("todolist")]
    public class ToDoListModel
    {
        [PrimaryKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name", Length = 100)]
        public string Name { get; set; }

        [Column("userid")]
        public Guid UserId { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.UserId))]
        public ToDoUserModel User { get; set; }
    }
}
