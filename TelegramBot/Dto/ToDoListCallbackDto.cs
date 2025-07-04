using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.TelegramBot.Dto
{
    public class ToDoListCallbackDto : CallbackDto
    {
        //string Action { get; set; }
        public Guid? ToDoListId { get; set; }

        //На вход принимает строку ввида "{action}|{toDoListId}|{prop2}...
        public static new ToDoListCallbackDto FromString(string input)
        {
            string[] strings = input.Split('|');
            Guid? toDoListId = null;
            if (Guid.TryParse(strings[1], out Guid guidId))
            {
                toDoListId = guidId;
            }
            return new ToDoListCallbackDto()
            {
                Action = strings[0],
                ToDoListId = toDoListId
            };
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoListId.ToString()}";
        }
    }
}
