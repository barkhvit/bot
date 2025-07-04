using Bot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.TelegramBot.Dto
{
    public class ToDoItemCallbackDto : CallbackDto
    {
        public Guid? toDoItemId { get; set; }

        public static new ToDoItemCallbackDto FromString(string query)
        {
            string[] Query = query.Split('|');
            Guid? guid = null;
            if (Query.Count() > 1)
            {
                if (Guid.TryParse(Query[1], out Guid guidId))
                {
                    guid = guidId;
                }
            }
            
            return new ToDoItemCallbackDto()
            {
                Action = Query[0],
                toDoItemId = guid
            };
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{toDoItemId.ToString()}";
        }
    }
}
