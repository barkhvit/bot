
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Entities
{
    public class ToDoUser
    {
        public Guid UserId { get; set; }
        public long telegramUserId { get; set; }
        public string TelegramUserName { get; set; }
        public DateTime RegisteredAt { get; set; }
    }
}
