using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Helpers
{
    public static class KeyBoards
    {
        public static ReplyKeyboardMarkup GetCancelKeyboard() => new(new[] { new KeyboardButton("/cancel") }) { ResizeKeyboard = true };
        public static ReplyKeyboardMarkup GetDefaultKeyboard() => new(new[] { new KeyboardButton("/addtask"), new KeyboardButton("/show") }) { ResizeKeyboard = true };
    }
}
