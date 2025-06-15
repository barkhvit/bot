using Bot.Core.Entities;
using Bot.Core.Services;
using Bot.Helpers;
using Bot.TelegramBot.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.TelegramBot.Scenarios
{
    public class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;
        private readonly IToDoListService _toDoListService;
        private readonly IScenarioContextRepository _scenarioContextRepository;
        public AddTaskScenario(IUserService userService, IToDoService toDoService, IScenarioContextRepository scenarioContextRepository, IToDoListService toDoListService)
        {
            _userService = userService;
            _toDoService = toDoService;
            _toDoListService = toDoListService;
            _scenarioContextRepository = scenarioContextRepository;
        }
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.AddTask;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            ScenarioResult scenarioResult;
            switch (context.CurrentStep) 
            {
                case null:
                    ToDoUser? user = await _userService.GetUser(update.Message.From.Id, ct);
                    context.Data["User"] = user;
                    await bot.SendMessage(
                        chatId: update.Message.Chat.Id,
                        text: "Введите название задачи:",
                        replyMarkup: Helpers.KeyBoards.GetCancelKeyboard(),
                        cancellationToken:ct);
                    context.CurrentStep = "Name";
                    scenarioResult = ScenarioResult.Transition; break;

                case "Name":
                    context.Data["Name"] = update.Message.Text;
                    var User = await _userService.GetUser(update.Message.From.Id, ct);
                    var lists = await _toDoListService.GetUserLists(User.UserId, ct);
                    await bot.SendMessage(
                        chatId: update.Message.Chat.Id,
                        text: "Выберите список:",
                        cancellationToken: ct,
                        replyMarkup: Dto.KeyBoards.KeyBoardForListsOnlyNames(lists,true));
                    context.CurrentStep = "List";
                    scenarioResult = ScenarioResult.Transition; break;

                case "List":
                    ToDoListCallbackDto toDoListCallbackDto = ToDoListCallbackDto.FromString(update.CallbackQuery.Data);
                    if (toDoListCallbackDto.ToDoListId == null)
                    {
                        context.Data["List"] = null;
                    }
                    else context.Data["List"] = await _toDoListService.Get((Guid)toDoListCallbackDto.ToDoListId,ct);

                    await bot.SendMessage(
                        chatId: update.CallbackQuery.Message.Chat.Id,
                        text: "Введите срок выполнения в формате: дд.мм.гггг",
                        cancellationToken: ct,
                        replyMarkup: InlineKeyboardButton.WithCallbackData("❌Отменить", "cancel"));
                    context.CurrentStep = "DeadLine";
                    scenarioResult = ScenarioResult.Transition; break;

                case "DeadLine": 
                    if(DateTime.TryParseExact(update.Message.Text,"dd.MM.yyyy",CultureInfo.InvariantCulture,DateTimeStyles.None, out var deadline))
                    {
                        var _user = (ToDoUser)context.Data["User"];
                        var Name = (string)context.Data["Name"];
                        var List = (ToDoList)context.Data["List"];
                        await _toDoService.Add(_user, Name, deadline, List, ct);
                        await bot.SendMessage(
                            chatId: update.Message.Chat.Id,
                            text: "Задача успешно добавлена!",
                            cancellationToken:ct,
                            replyMarkup: Helpers.KeyBoards.GetDefaultKeyboard());
                        context.CurrentStep = "List";
                        scenarioResult = ScenarioResult.Completed;
                    }
                    else
                    {
                        await bot.SendMessage(
                            chatId: update.Message.Chat.Id,
                            text: "Неверный формат даты. Пожалуйста, введите дату в формате dd.MM.yyyy:",
                            replyMarkup: Helpers.KeyBoards.GetCancelKeyboard(),
                            cancellationToken:ct);
                        scenarioResult = ScenarioResult.Transition;
                    }
                    break;

                

                default: scenarioResult = ScenarioResult.Transition; break;
            }

            return scenarioResult;
        }

    }
}
