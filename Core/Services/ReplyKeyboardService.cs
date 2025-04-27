using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Core.Services
{
    public static class ReplyKeyboardService
    {
        //клавиатура для незарегистрированных пользователей
        public static ReplyKeyboardMarkup GetStartKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[]{"/start","/info"}
            })
            {
                ResizeKeyboard = true
            };
        }

        //клавиатура для зарегистрированных пользователей
        public static ReplyKeyboardMarkup GetAllCommandKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[]{"/help", "/info" },
                new KeyboardButton[]{ "/showtasks", "/showalltasks"},
                new KeyboardButton[]{"/report"}
            })
            {
                ResizeKeyboard = true
            };
        }
    }
}
