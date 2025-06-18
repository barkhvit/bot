using Bot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.TelegramBot.Dto
{
    public static class KeyBoards
    {
        //клавиатура 
        public static InlineKeyboardMarkup KeyBoardForLists(IReadOnlyList<ToDoList> toDoLists)
        {
            var buttons = new List<IEnumerable<InlineKeyboardButton>>();

            buttons.Add(new[] {InlineKeyboardButton.WithCallbackData("📌 Без списка", new ToDoListCallbackDto { Action = "show", ToDoListId = null }.ToString()) });
            foreach(ToDoList toDoList in toDoLists)
            {
                buttons.Add(new[] {InlineKeyboardButton.WithCallbackData(toDoList.Name,
                    new ToDoListCallbackDto { Action = "show", ToDoListId = toDoList.Id}.ToString()) });
            }
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("🆕Добавить", "addlist") });
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("❌Удалить", "deletelist") });

            return new InlineKeyboardMarkup(buttons);
        }

        public static InlineKeyboardMarkup KeyBoardForListsOnlyNames(IReadOnlyList<ToDoList> toDoLists, bool withNoList)
        {
            var buttons = new List<IEnumerable<InlineKeyboardButton>>();

            if(withNoList) buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("📌 Без списка", 
                new ToDoListCallbackDto { Action = "show", ToDoListId = null }.ToString()) });

            foreach (ToDoList toDoList in toDoLists)
            {
                buttons.Add(new[] {InlineKeyboardButton.WithCallbackData(toDoList.Name,
                    new ToDoListCallbackDto { Action = "deletelist", ToDoListId = toDoList.Id}.ToString()) });
            }
            
            return new InlineKeyboardMarkup(buttons);
        }

        public static InlineKeyboardMarkup KeyBoardYesNo()
        {
            var buttons = new List<InlineKeyboardButton>();

            buttons.Add(InlineKeyboardButton.WithCallbackData("✅Да", "yes"));
            buttons.Add(InlineKeyboardButton.WithCallbackData("❌Нет", "no"));

            return new InlineKeyboardMarkup(buttons);
        }
    }
}
