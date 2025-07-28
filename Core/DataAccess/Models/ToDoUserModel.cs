using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.DataAccess.Models
{
    [Table("todouser")]
    public class ToDoUserModel
    {
        [PrimaryKey]
        [Column("userid")]
        public Guid UserId { get; set; }

        [Column("telegramuserid")]
        public long TelegramUserId { get; set; }

        [Column("telegramusername", Length = 100)]
        public string TelegramUserName { get; set; }

        [Column("registeredat")]
        public DateTime RegisteredAt { get; set; }
    }
}
