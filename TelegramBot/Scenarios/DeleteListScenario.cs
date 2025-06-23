using Bot.Core.Entities;
using Bot.Core.Services;
using Bot.Helpers;
using Bot.TelegramBot.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.TelegramBot.Scenarios
{
    public class DeleteListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _toDoListService;
        private readonly IToDoService _toDoService;
        private readonly IScenarioContextRepository _scenarioContextRepository;

        public DeleteListScenario(IUserService userService, IToDoListService toDoListService, IScenarioContextRepository scenarioContextRepository, IToDoService toDoService)
        {
            _userService = userService;
            _toDoListService = toDoListService;
            _toDoService = toDoService;
            _scenarioContextRepository = scenarioContextRepository;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.DeleteList;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            ScenarioResult scenarioResult;
            switch (context.CurrentStep)
            {
                case null:
                    ToDoUser? user = await _userService.GetUser(update.CallbackQuery.From.Id, ct);
                    context.Data["User"] = user;
                    await bot.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
                    //вывести inline кнопки с листами
                    var lists = await _toDoListService.GetUserLists(user.UserId, ct);
                    InlineKeyboardMarkup inlineKeyboardMarkup = Dto.KeyBoards.KeyBoardForListsOnlyNames(lists,false);
                    await bot.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
                    await bot.SendMessage(
                        chatId: update.CallbackQuery.Message.Chat.Id,
                        text: "Выберете список для удаления:",
                        replyMarkup: inlineKeyboardMarkup,
                        cancellationToken: ct);
                    context.CurrentStep = "Approve";
                    scenarioResult = ScenarioResult.Transition; break;

                case "Approve":
                    await bot.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
                    ToDoListCallbackDto toDoListCallbackDto = ToDoListCallbackDto.FromString(update.CallbackQuery.Data);
                    Guid listId = (Guid)toDoListCallbackDto.ToDoListId;
                    var list = await _toDoListService.Get(listId, ct);
                    context.Data["List"] = list;

                    await bot.SendMessage(
                            chatId: update.CallbackQuery.Message.Chat.Id,
                            text: $"Вы точно хотите удалить список: {list.Name}?",
                            cancellationToken: ct,
                            replyMarkup: Dto.KeyBoards.KeyBoardYesNo());
                    scenarioResult = ScenarioResult.Transition;
                    context.CurrentStep = "delete";
                    break;

                case "delete":
                    await bot.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
                    string mesText = "";
                    switch (update.CallbackQuery.Data) 
                    {
                        case "yes":
                            var List = (ToDoList)context.Data["List"];
                            var User = await _userService.GetUser(update.CallbackQuery.From.Id, ct);
                            var ToDoItems = await _toDoService.GetAllByUserIdAndList(User.UserId, List.Id, ct);
                            ToDoItems = ToDoItems.Where(i => i.State == ToDoItemState.Active).ToList();
                            foreach (var items in ToDoItems) await _toDoService.Delete(items.Id, ct);
                            await _toDoListService.Delete(List.Id, ct);
                            mesText = $"Задачи из списка \"{List.Name}\" удалены.";
                            break;
                        case "no":
                            mesText = "Удаление отменено.";
                            break;
                    }
                    await bot.SendMessage(
                        chatId: update.CallbackQuery.Message.Chat.Id,
                        text: mesText,
                        cancellationToken: ct);

                    scenarioResult = ScenarioResult.Completed;
                    break;


                default: scenarioResult = ScenarioResult.Transition; break;
            }

            return scenarioResult;
        }
    }
}
