using Bot.Core.Entities;
using Bot.Helpers;
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
        private static int _pageSize = 5;

        //клавиатура с названием листов и кнопками удалить и добавить
        public static InlineKeyboardMarkup KeyBoardForLists(IReadOnlyList<ToDoList> toDoLists)
        {
            var buttons = new List<IEnumerable<InlineKeyboardButton>>();

            buttons.Add(new[] {InlineKeyboardButton.WithCallbackData("📌 Без списка", new PagedListCallbackDto { Action = "show", ToDoListId = null }.ToString()) });
            foreach(ToDoList toDoList in toDoLists)
            {
                buttons.Add(new[] {InlineKeyboardButton.WithCallbackData(toDoList.Name,
                    new PagedListCallbackDto { Action = "show", ToDoListId = toDoList.Id, Page = 0}.ToString()) });
            }
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("🆕Добавить", "addlist") });
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("❌Удалить", "deletelist") });

            return new InlineKeyboardMarkup(buttons);
        }

        //клавиатура только с названиями листов
        public static InlineKeyboardMarkup KeyBoardForListsOnlyNames(IReadOnlyList<ToDoList> toDoLists, bool withNoList)
        {
            var buttons = new List<IEnumerable<InlineKeyboardButton>>();

            if(withNoList) buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("📌 Без списка", 
                new PagedListCallbackDto { Action = "show", ToDoListId = null }.ToString()) });

            foreach (ToDoList toDoList in toDoLists)
            {
                buttons.Add(new[] {InlineKeyboardButton.WithCallbackData(toDoList.Name,
                    new PagedListCallbackDto { Action = "deletelist", ToDoListId = toDoList.Id}.ToString()) });
            }
            
            return new InlineKeyboardMarkup(buttons);
        }

        //клавиатура да и нет
        public static InlineKeyboardMarkup KeyBoardYesNo()
        {
            var buttons = new List<InlineKeyboardButton>();

            buttons.Add(InlineKeyboardButton.WithCallbackData("✅Да", "yes"));
            buttons.Add(InlineKeyboardButton.WithCallbackData("❌Нет", "no"));

            return new InlineKeyboardMarkup(buttons);
        }

        //выводит список задач в выбраном списке
        internal static InlineKeyboardMarkup KeyBoardForToDoItems(List<ToDoItem>? toDoItems, Guid? listId, ToDoItemState itemStateButton,CancellationToken ct)
        {
            var buttons = new List<IEnumerable<InlineKeyboardButton>>();
            if (toDoItems != null)
            {
                foreach (var item in toDoItems)
                { 
                    buttons.Add(new[]
                    {
                        InlineKeyboardButton.WithCallbackData(item.Name,
                            new ToDoItemCallbackDto{Action = "showtask", toDoItemId = item.Id }.ToString())
                    });
                }
                
            }
            // в зависимости от itemState делаем кнопку (посмотреть активные или выполненные
            InlineKeyboardButton buttonStateItem = itemStateButton == ToDoItemState.Active ?
                InlineKeyboardButton.WithCallbackData("☑️Посмотреть выполненные", new ToDoListCallbackDto { Action = "show_completed", ToDoListId = listId }.ToString()) :
                InlineKeyboardButton.WithCallbackData("Посмотреть активные", new ToDoListCallbackDto { Action="show", ToDoListId=listId}.ToString());

            buttons.Add(new[]{buttonStateItem});

            return new InlineKeyboardMarkup(buttons);
        }

        //клавиатура для задач с постраничной навигацией
        public static InlineKeyboardMarkup BuildPagedButtons(IReadOnlyList<KeyValuePair<string, string>> callbackData, PagedListCallbackDto listDto)
        {
            // Рассчитываем общее количество страниц
            int totalPages = (int)Math.Ceiling((double)callbackData.Count / _pageSize);

            //получаем кнопки для текущей страницы
            var items = callbackData.GetBatchByNumber(_pageSize, listDto.Page);
            var buttons = items.Select(i => new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(i.Key, i.Value) }).ToList();


            //кнопки навигации если нужно
            // Кнопка "Назад" (если не на первой странице)
            if (listDto.Page > 0)
            {
                buttons.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(
                    "⬅️", new PagedListCallbackDto(){Action = "show", ToDoListId = listDto.ToDoListId, Page = listDto.Page-1}.ToString()) });
            }
            // Кнопка "Вперед" (если не на последней странице)
            if (listDto.Page < totalPages - 1)
            {
                buttons.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(
                    "➡️", new PagedListCallbackDto(){Action = "show", ToDoListId = listDto.ToDoListId, Page = listDto.Page+1}.ToString()) });
            }

            return new InlineKeyboardMarkup(buttons);
        }


    } 
}
