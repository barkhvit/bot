using Bot.Commands;
using Bot.Core.Services;
using Bot.TelegramBot.Dto;
using Bot.TelegramBot.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace Bot.TelegramBot
{
    public class CallBackUpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoReportService _toDoReportService;
        private readonly IScenarioContextRepository _scenarioContextRepository;
        private readonly IEnumerable<IScenario> _scenarios;
        private readonly IToDoListService _toDoListService;
        private readonly ToDoCommands _toDoCommands;

        public CallBackUpdateHandler(ITelegramBotClient telegramBotClient, IUserService userService, IToDoService todoService, 
            IToDoReportService toDoReportService, IEnumerable<IScenario> scenarios, IScenarioContextRepository contextRepository, IToDoListService toDoListService)
        {
            _telegramBotClient = telegramBotClient;
            _userService = userService;
            _todoService = todoService;
            _toDoReportService = toDoReportService;
            _scenarios = scenarios;
            _scenarioContextRepository = contextRepository;
            _toDoListService = toDoListService;
            _toDoCommands = new ToDoCommands(_telegramBotClient, _todoService);
        }

        private IScenario GetScenario(ScenarioType scenario)
        {
            var handler = _scenarios.FirstOrDefault(s => s.CanHandle(scenario));
            return handler ?? throw new InvalidOperationException($"Нет подходящего сценария для типа: {scenario}");
        }

        private async Task ProcessScenario(ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenario = GetScenario(context.CurrentScenario);
            var result = await scenario.HandleMessageAsync(_telegramBotClient, context, update, ct);
            if (result == ScenarioResult.Completed)
            {
                await _scenarioContextRepository.ResetContext(context.UserId, ct);
            }
            else
            {
                await _scenarioContextRepository.SetContext(context.UserId, context, ct);
            }
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            var user = await _userService.RegisterUser(update.CallbackQuery.From.Id, update.CallbackQuery.From.Username, ct);
            CallbackDto dto = CallbackDto.FromString(update.CallbackQuery.Data);
            try
            {
                switch (dto.Action)
                {
                    case "show":
                        ToDoListCallbackDto toDoListCallbackDto = ToDoListCallbackDto.FromString(update.CallbackQuery.Data);
                        await _toDoCommands.ShowToDoItemsByUserIdandListId(update, user.UserId, toDoListCallbackDto.ToDoListId, ct);
                        break;
                    case "addlist":
                        var newContext = new ScenarioContext(update.CallbackQuery.From.Id, ScenarioType.AddList);
                        await _scenarioContextRepository.SetContext(update.CallbackQuery.From.Id, newContext, ct);
                        await ProcessScenario(newContext, update, ct);
                        break;
                    case "deletelist":
                        var deleteListContext = new ScenarioContext(update.CallbackQuery.From.Id, ScenarioType.DeleteList);
                        await _scenarioContextRepository.SetContext(update.CallbackQuery.From.Id, deleteListContext, ct);
                        await ProcessScenario(deleteListContext, update, ct);
                        break;
                }
            }
            catch
            {
                throw;
            }
        }




        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        
    }
}
