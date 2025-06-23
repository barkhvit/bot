using Bot.Core.Entities;
using Bot.Core.Services;
using Bot.TelegramBot.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Commands
{
    public class ToDoCommands
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IToDoService _toDoService;

        public ToDoCommands(ITelegramBotClient telegramBotClient, IToDoService toDoService)
        {
            _telegramBotClient = telegramBotClient;
            _toDoService = toDoService;
        }

        //выводим список всех активных/завершенных задач в виде инлайн кнопок и кнопку показать завершенные или активные
        public async Task ShowToDoItemsByUserIdandListId(PagedListCallbackDto pagedListCallbackDto, Update update, Guid userId, Guid? listId,ToDoItemState toDoItemState, CancellationToken ct)
        {
            //все задачи пользователя по листу
            var items = await _toDoService.GetAllByUserIdAndList(userId, listId, ct);

            //только активные или завершенные
            var Items = items.Where(i => i.State == toDoItemState).ToList();
            string state = toDoItemState == ToDoItemState.Active ? "Активны" : "Завершенны";
            string mesText = Items == null || Items.Count==0 ? $"{state}х задач нет" : $"{state}е задачи:";
            InlineKeyboardMarkup inlineKeyboardMarkup = KeyBoards.KeyBoardForToDoItems(Items, listId , toDoItemState, ct);

            //создаем пары (название задачи | ToDoItemCallbackDto)
            var callbackData = Items.Select(i => new KeyValuePair<string, string>(i.Name, new ToDoItemCallbackDto() 
                {Action="showtask",toDoItemId = i.Id}.ToString())).ToList().AsReadOnly();

            //делаем клавиатуру с задачами с постраничной навигацией
            InlineKeyboardMarkup inlineKeyboardMarkup1 = KeyBoards.BuildPagedButtons(callbackData,pagedListCallbackDto);

            // в зависимости от itemState делаем кнопку (посмотреть активные или выполненные
            InlineKeyboardButton buttonStateItem = toDoItemState == ToDoItemState.Active ?
                InlineKeyboardButton.WithCallbackData("☑️Посмотреть выполненные", new ToDoListCallbackDto { Action = "show_completed", ToDoListId = listId }.ToString()) :
                InlineKeyboardButton.WithCallbackData("Посмотреть активные", new ToDoListCallbackDto { Action = "show", ToDoListId = listId }.ToString());

            var keyboardRows = inlineKeyboardMarkup1.InlineKeyboard.ToList();
            keyboardRows.Add(new[] { buttonStateItem });
            inlineKeyboardMarkup1 = new InlineKeyboardMarkup(keyboardRows);


            await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
            await _telegramBotClient.EditMessageText(
                messageId: update.CallbackQuery.Message.MessageId,
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: mesText,
                replyMarkup: inlineKeyboardMarkup1,
                cancellationToken: ct);
        }


        // Метод для экранирования специальных символов MarkdownV2
        private static string EscapeMarkdownV2(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            char[] specialChars = { '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' };
            var builder = new StringBuilder();

            foreach (char c in text)
            {
                if (specialChars.Contains(c))
                {
                    builder.Append('\\');
                }
                builder.Append(c);
            }

            return builder.ToString();
        }

        internal async Task ShowDetailToDoItems(Update update, Guid toDoItemId, CancellationToken ct)
        {
            var toDoItem = await _toDoService.Get(toDoItemId, ct);
            if (toDoItem != null)
            {
                string mesText = $"{toDoItem.Name}\n\nСрок выполнения: {toDoItem.Deadline.ToString("dd.MM.yyyy")}\n" +
                    $"Дата создания: {toDoItem.CreatedAt.ToString("dd.MM.yyyy")}";
                await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken:ct);
                await _telegramBotClient.EditMessageText(
                    messageId: update.CallbackQuery.Message.MessageId,
                    chatId: update.CallbackQuery.From.Id,
                    text: mesText,
                    cancellationToken: ct,
                    replyMarkup: new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("✅Выполнить",
                            new ToDoItemCallbackDto(){Action = "completetask", toDoItemId = toDoItem.Id}.ToString()),
                        InlineKeyboardButton.WithCallbackData("❌Удалить",
                            new ToDoItemCallbackDto(){Action = "deletetask", toDoItemId = toDoItem.Id}.ToString())
                    });
            }
        }

        internal async Task MakeToDoItemCompleted(Update update, Guid toDoItemId, CancellationToken ct)
        {
            await _toDoService.MarkCompleted(toDoItemId, ct);
            await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
            await _telegramBotClient.EditMessageText(
                messageId: update.CallbackQuery.Message.MessageId,
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: "Задача завершена",
                cancellationToken: ct);
        }

        internal async Task DeleteToDoItem(Update update, Guid toDoItemId, CancellationToken ct)
        {
            await _toDoService.Delete(toDoItemId, ct);
            await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
            await _telegramBotClient.EditMessageText(
                messageId: update.CallbackQuery.Message.MessageId,
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: "Задача удалена.",
                cancellationToken: ct);
        }
    }

    
}
