using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.TelegramBot.Dto
{
    public class CallbackDto
    {
        //с помощью него будет определять за какое действие отвечает кнопка
        public string Action { get; set; }

        public static CallbackDto FromString(string input)
        {
            string[] strings = input.Split('|');
            return new CallbackDto() { Action = strings[0] };
        }

        public override string ToString()
        {
            return Action;
        }
    }
}
