using Bot.Core.Entities;
using Bot.Core.Services;
using Bot.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.TelegramBot.Scenarios
{
    public class AddListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _toDoListService;
        private readonly IScenarioContextRepository _scenarioContextRepository;

        public AddListScenario(IUserService userService, IToDoListService toDoListService, IScenarioContextRepository scenarioContextRepository)
        {
            _userService = userService;
            _toDoListService = toDoListService;
            _scenarioContextRepository = scenarioContextRepository;
        }
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.AddList;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            ScenarioResult scenarioResult;
            switch (context.CurrentStep)
            {
                case null:
                    ToDoUser? user = await _userService.GetUser(update.CallbackQuery.From.Id, ct);
                    context.Data["User"] = user;
                    await bot.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
                    await bot.SendMessage(
                        chatId: update.CallbackQuery.Message.Chat.Id,
                        text: "Введите название списка:",
                        replyMarkup: KeyBoards.GetCancelKeyboard(),
                        cancellationToken: ct);
                    context.CurrentStep = "Name";
                    scenarioResult = ScenarioResult.Transition; break;

                case "Name":
                    if (update.Message.Text != null)
                    {
                        string listName = update.Message.Text;
                        var User = (ToDoUser)context.Data["User"];
                        var list = await _toDoListService.Add(User, listName, ct);

                        await bot.SendMessage(
                            chatId: update.Message.Chat.Id,
                            text: $"Список {listName} добавлен.",
                            cancellationToken: ct);

                        scenarioResult = ScenarioResult.Completed; break;
                    }

                    await bot.SendMessage(
                            chatId: update.CallbackQuery.Message.Chat.Id,
                            text: "Введите название списка:",
                            cancellationToken: ct,
                            replyMarkup: KeyBoards.GetCancelKeyboard());
                    scenarioResult = ScenarioResult.Transition; break;

                default: scenarioResult = ScenarioResult.Transition; break;
            }

            return scenarioResult;
        }
    }
}
