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

        public async Task ShowToDoItemsByUserIdandListId(Update update, Guid userId, Guid? listId, CancellationToken ct)
        {
            var items = await _toDoService.GetByUserIdAndList(userId, listId, ct);
            string mesText = "";
            if (items == null || items.Count==0) mesText = "Задач нет.";

            else
            {
                mesText = "Список активных задач:";
                foreach(var item in items)
                {
                    // Экранируем все специальные символы MarkdownV2
                    string escapedName = EscapeMarkdownV2(item.Name);
                    string escapedDeadline = EscapeMarkdownV2(item.Deadline.ToString("dd\\.MM\\.yyyy")); // Экранируем точки
                    string escapedId = EscapeMarkdownV2(item.Id.ToString());

                    mesText += $"\n  {escapedName}, Deadline: {escapedDeadline}, Id: `{escapedId}`";
                }
            }
            await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
            await _telegramBotClient.SendMessage(
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: mesText,
                cancellationToken: ct,
                parseMode: ParseMode.MarkdownV2);
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
    }

    
}
