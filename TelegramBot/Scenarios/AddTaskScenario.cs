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
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.TelegramBot.Scenarios
{
    public class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;
        private readonly IScenarioContextRepository _scenarioContextRepository;
        public AddTaskScenario(IUserService userService, IToDoService toDoService, IScenarioContextRepository scenarioContextRepository)
        {
            _userService = userService;
            _toDoService = toDoService;
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
                        replyMarkup: KeyBoards.GetCancelKeyboard(),
                        cancellationToken:ct);
                    context.CurrentStep = "Name";
                    scenarioResult = ScenarioResult.Transition; break;

                case "Name":
                    context.Data["Name"] = update.Message.Text;
                    await bot.SendMessage(
                        chatId: update.Message.Chat.Id,
                        text: "Введите срок выполнения задачи в формате dd.MM.yyyy:",
                        cancellationToken: ct,
                        replyMarkup: KeyBoards.GetCancelKeyboard());
                    context.CurrentStep = "DeadLine";
                    scenarioResult = ScenarioResult.Transition; break;

                case "DeadLine": 
                    if(DateTime.TryParseExact(update.Message.Text,"dd.MM.yyyy",CultureInfo.InvariantCulture,DateTimeStyles.None, out var deadline))
                    {
                        var User = (ToDoUser)context.Data["User"];
                        var Name = (string)context.Data["Name"];
                        await _toDoService.Add(User, Name, deadline, ct);
                        await bot.SendMessage(
                            chatId: update.Message.Chat.Id,
                            text: "Задача успешно добавлена!",
                            cancellationToken:ct,
                            replyMarkup: KeyBoards.GetDefaultKeyboard());
                        scenarioResult = ScenarioResult.Completed;
                    }
                    else
                    {
                        await bot.SendMessage(
                            chatId: update.Message.Chat.Id,
                            text: "Неверный формат даты. Пожалуйста, введите дату в формате dd.MM.yyyy:",
                            replyMarkup: KeyBoards.GetCancelKeyboard(),
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
