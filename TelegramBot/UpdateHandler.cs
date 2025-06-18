using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bot.Core.Entities;
using Bot.Core.Exceptions;
using Bot.Core.Services;
using static System.Net.Mime.MediaTypeNames;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using System.Collections;
using Bot.TelegramBot.Scenarios;
using Bot.TelegramBot.Dto;

namespace Bot.TelegramBot
{
    public delegate void MessageEventHandler(string message);
    public class UpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoReportService _toDoReportService;
        private readonly string CommandsForRegUser = "\n /addtask \n /removetask \n /completetask \n /find";
        private readonly IScenarioContextRepository _scenarioContextRepository;
        private readonly IEnumerable<IScenario> _scenarios;
        private readonly IToDoListService _toDoListService;
        private readonly CallBackUpdateHandler _callBackUpdateHandler;
        //private readonly string CommandsForRegUser = "\n /info \n /help \n /addtask \n /removetask \n /completetask \n /showtasks \n /showalltasks \n /find";//список команд для зарегистрированного пользователя


        //события 
        public event MessageEventHandler? OnHandleUpdateStarted;
        public event MessageEventHandler? OnHandleUpdateCompleted;

        //КОНСТРУКТОР
        public UpdateHandler(ITelegramBotClient telegramBotClient, IUserService userService, IToDoService todoService, IToDoReportService toDoReportService,
            IEnumerable<IScenario> scenarios, IScenarioContextRepository contextRepository, IToDoListService toDoListService)
        {   
            _telegramBotClient = telegramBotClient;
            _userService = userService;
            _todoService = todoService;
            _toDoReportService = toDoReportService;
            _scenarios = scenarios;
            _scenarioContextRepository = contextRepository;
            _toDoListService = toDoListService;
            _callBackUpdateHandler = new CallBackUpdateHandler(telegramBotClient, userService, todoService, toDoReportService, scenarios, contextRepository, toDoListService);
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
            if(result == ScenarioResult.Completed)
            {
                await _scenarioContextRepository.ResetContext(context.UserId, ct);
            }
            else
            {
                await _scenarioContextRepository.SetContext(context.UserId, context, ct);
            }
        }


        //Обработчик команд от пользователя 
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, Text) = update switch
            {
                { Type: UpdateType.Message, Message: var msg }
                    => (msg.Chat.Id, msg.From.Id, msg.MessageId,msg.Text),

                { Type: UpdateType.CallbackQuery, CallbackQuery: var cbq }
                    => (cbq.Message.Chat.Id, cbq.From.Id, cbq.Message.MessageId, cbq.Data),

                _ => throw new InvalidOperationException("Неизвестный тип сообщения от пользователя")
            };

            string[] _text = Text.Split(' ',2); //сообщение от пользователя помещаем в двумерный массив
            ToDoUser? _user = await _userService.GetUser(userId, cancellationToken);//присваиваем пользователя локальной переменной

            // Обработка команды /cancel до начала сценария
            if (Text == "/cancel" || Text == "cancel")
            {
                await _scenarioContextRepository.ResetContext(userId, cancellationToken);
                await _telegramBotClient.SendMessage(
                    chatId,
                    "Текущее действие отменено.",
                    replyMarkup: ReplyKeyboardService.GetAllCommandKeyboard(),
                    cancellationToken: cancellationToken);
                return;
            }


            // Проверка активного сценария у пользователя
            var context = await _scenarioContextRepository.GetContext(userId, cancellationToken);
            if (context != null)
            {
                await ProcessScenario(context, update, cancellationToken);
                return;
            }

            // Вызываем событие "Началась обработка"
            OnHandleUpdateStarted?.Invoke(Text);

            

            try
            {
                //обработка CallBackQuery
                if (update.Type == UpdateType.CallbackQuery)
                {
                    _callBackUpdateHandler.HandleUpdateAsync(_telegramBotClient, update, cancellationToken);
                }
                //обработка Message
                else
                {
                    if (_text.Length == 0) throw new IncorrectInputException();

                    //обработка команды от пользователя
                    switch (_text[0])
                    {
                        case "/start": await StartCommand(_user, update, cancellationToken); break;
                        case "/help": await ComandHelp(_user, update, cancellationToken); break;
                        case "/info": await botClient.SendMessage(update.Message.Chat, "| Версия программы: 2.0 | Дата создания: 30.03.2025 | Автор: Бархатов Виталий", cancellationToken: cancellationToken); ; break;
                        case "/addtask": await AddTaskCommand(_user, update, _text, cancellationToken); break;
                        //case "/show": await ShowTasksCommand(_user, update, cancellationToken); break;
                        case "/show": await ShowListsCommand(_user, update, cancellationToken); break;
                        case "/completetask": await CompleteTaskCommand(_user, update, _text, cancellationToken); break;
                        case "/removetask": await RemoveTaskCommand(_user, update, _text, cancellationToken); break;
                        case "/report": await ReportCommand(_user, update, _text, cancellationToken); break;
                        case "/find": await FindCommandAsync(_user, update, _text, cancellationToken); break;
                        case "/showusers": break;
                        case "/exit": break;
                        default: throw new IncorrectInputException();
                    }
                }

            }
            catch (ArgumentException) //некорректный аргумент при вводе кол-ва задач и символов в задаче или задача == null
            {
                await botClient.SendMessage(update.Message.Chat, "Некорректный ввод.", cancellationToken: cancellationToken);
            }
            catch (IncorrectTaskException e) //некорректный ввод после команды /addtask
            {
                await botClient.SendMessage(update.Message.Chat, $"{e.Message}", cancellationToken: cancellationToken);
            }
            catch (TaskLengthLimitException e)//превышен лимит по символам в задаче
            {
                await botClient.SendMessage(update.Message.Chat, $"{e.Message}", cancellationToken: cancellationToken);
            }
            catch (IncorrectArgumentTaskException e)//некорректно введен аргумент для команды /removetask
            {
                string message = "";
                switch (e.Type)
                {
                    case "/find": message = "строку для поиска";break;
                    case "/completetask": message = "GuidId задачи"; break;
                    case "/removetask": message = "GuidId задачи"; break;
                }
                await botClient.SendMessage(update.Message.Chat, $"Ошибка ввода. После команды {e.Type} через пробел необходимо ввести {message}.", cancellationToken: cancellationToken);
            }
            catch (DuplicateTaskException e)//если дубликат задачи
            {
                await botClient.SendMessage(update.Message.Chat, $"{e.Message}", cancellationToken: cancellationToken);
            }
            catch (TaskCountLimitException e)//превышен лимит кол-ва задач
            {
                await botClient.SendMessage(update.Message.Chat, $"{e.Message}", cancellationToken: cancellationToken);
            }
            catch (UserIsNotRegistratedException)
            {
                await botClient.SendMessage(
                    update.Message.Chat,
                    $"для начала работы введите /start",
                    replyMarkup: ReplyKeyboardService.GetStartKeyboard(),
                    cancellationToken: cancellationToken);
            }
            catch (NoтExistentTaskException)
            {
                await botClient.SendMessage(update.Message.Chat, $"Нет задачи с таким GuidId", cancellationToken: cancellationToken);
            }
            catch (IncorrectInputException)
            {
                await botClient.SendMessage(update.Message.Chat, $"Некорректный ввод команды", cancellationToken: cancellationToken);
            }
            catch (AddTodoListException ex)
            {
                await botClient.SendMessage(update.Message.Chat, ex.Message, cancellationToken: cancellationToken);
            }
            finally
            {
                //событие "конец обработки"
                OnHandleUpdateCompleted?.Invoke(Text);
            }
        }


        //=====================================================================================================================================================

        private async Task ShowListsCommand(ToDoUser? user, Update update, CancellationToken ct)
        {
            if(user == null) throw new UserIsNotRegistratedException();//если пользователь не зарегистрирован
            var toDoLists = await _toDoListService.GetUserLists(user.UserId, ct);
            InlineKeyboardMarkup inlineKeyboardMarkup = KeyBoards.KeyBoardForLists(toDoLists);
            await _telegramBotClient.SendMessage(
                chatId: update.Message.Chat.Id,
                text: "Выберите список:",
                cancellationToken:ct,
                replyMarkup:inlineKeyboardMarkup);
        }



        //поиск задачи 
        private async Task FindCommandAsync(ToDoUser? user, Update update, string[] text, CancellationToken cancellationToken)
        {
            if (user == null) throw new UserIsNotRegistratedException();//если пользователь не зарегистрирован
            if (text.Length == 1 || text[1] == "") throw new IncorrectArgumentTaskException("/find");
            ValidateString(text[1]);
            var itemsResult = await _todoService.Find(user, text[1], cancellationToken);
            string str = "Таких задач нет";
            if (itemsResult.Count != 0)
            {
                str = "Результат поиска:";
                foreach (ToDoItem Item in itemsResult)
                {
                    str = str + $"\n({Item.State}) {Item.Name} - {Item.CreatedAt} - `{Item.Id}`";
                }
            }
            await _telegramBotClient.SendMessage(update.Message.Chat, str, cancellationToken: cancellationToken, parseMode: ParseMode.MarkdownV2);
        }

        
        private async Task StartCommand(ToDoUser? _user, Update update, CancellationToken cancellationToken) //команда старт
        {
            // регистрируем пользователя
            _user = await _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username, cancellationToken);

            //добавляем главное меня бота
            await _telegramBotClient.SetMyCommands(
                commands: SetMyCommandsService.mainCommands,
                scope:null, //для всех пользователей,
                languageCode:null);//язык по умолчанию

            //добавляем кнопки с командами
            await _telegramBotClient.SendMessage(
                update.Message.Chat,
                $"Доступные команды: {CommandsForRegUser}",
                replyMarkup: ReplyKeyboardService.GetAllCommandKeyboard(),
                cancellationToken: cancellationToken);
        }
        
       

        private async Task AddTaskCommand(ToDoUser? _user, Update update, string[] _text, CancellationToken cancellationToken)//ДОБАВИТЬ задачу
        {
            var newContext = new ScenarioContext(update.Message.From.Id, ScenarioType.AddTask);
            await _scenarioContextRepository.SetContext(update.Message.From.Id, newContext, cancellationToken);
            await ProcessScenario(newContext, update, cancellationToken);
            //ToDoItem toDoItem = await _todoService.Add(_user, _text, cancellationToken);
            //await _telegramBotClient.SendMessage(update.Message.Chat, $"Задача \"{toDoItem.Name}\" добавлена, GuidId: `{toDoItem.Id}`", cancellationToken: cancellationToken, parseMode: ParseMode.MarkdownV2);
        }
        
        private async Task ComandHelp(ToDoUser? _user, Update update, CancellationToken cancellationToken)//команда help
        {
            string str = "Для начала работы введите команду /start";
            if (_user != null)
                str = "Доступные команды:\n /addtask название задачи - добавить новую задачу" +
                    "\n /removetask Id - удалить задачу" +
                    " \n /completetask Id - завершить задачу" +
                    " \n /show - показать активные задачи" +
                    " \n /find + начало задачи - поиск задач по началу" +
                    " \n /report - отчет по задачам";
            await _telegramBotClient.SendMessage(update.Message.Chat, str, cancellationToken: cancellationToken);
        }

        //приводит строку к int и проверяет, что оно находится в диапазоне min и max
        public static int ParseAndValidateInt(string? str, int min, int max)
        {
            if (int.TryParse(str, out int result))
            {
                if (result <= max && result >= min)
                    return result;
            }
            throw new ArgumentException();
        }

        //проверяет, что строка не равна null, не равна пустой строке и имеет какие-то символы кроме пробела
        public static void ValidateString(string? str)
        {
            if (string.IsNullOrEmpty(str))//проверяем, что строка не ноль не пустая
                throw new ArgumentException();

            int i = str.Replace(" ", "").Length;
            if (i == 0) throw new ArgumentException();//проверяем что строка не состоит только из пробелов
        }

        //УДАЛИТЬ задачу
        private async Task RemoveTaskCommand(ToDoUser? user, Update update, string[] _text, CancellationToken cancellationToken)
        {
            if (user == null) throw new UserIsNotRegistratedException();//если пользователь не зарегистрирован
            if (_text.Length == 1 || _text[1] == "") throw new IncorrectArgumentTaskException("/removetask");
            ValidateString(_text[1]);

            if (Guid.TryParse(_text[1], out var guid))//проверяем корректность GUID и есть ли задача с таким GUID
            {
                await _todoService.Delete(guid, cancellationToken);
                await _telegramBotClient.SendMessage(update.Message.Chat, $"Задача GuidId: \"{_text[1]}\" удалена ", cancellationToken: cancellationToken);
            }
            else
                await _telegramBotClient.SendMessage(update.Message.Chat, $"Задачи с GuidId \"{_text[1]}\" не cуществует ", cancellationToken: cancellationToken);
        }

        //переводит задачу в статус ВЫПОЛНЕНО
        private async Task CompleteTaskCommand(ToDoUser? user, Update update, string[] _text, CancellationToken cancellationToken) 
        {
            if (user == null) throw new UserIsNotRegistratedException();//если пользователь не зарегистрирован
            if (_text.Length == 1 || _text[1] == "") throw new IncorrectArgumentTaskException("/completetask");
            ValidateString(_text[1]);

            if (Guid.TryParse(_text[1], out var guid))//проверяем корректность GUID
            {
                await _todoService.MarkCompleted(guid, user.UserId, cancellationToken);
                await _telegramBotClient.SendMessage(update.Message.Chat, $"Задача GuidId: \"{_text[1]}\" выполнена ", cancellationToken: cancellationToken);
            }
            else
                await _telegramBotClient.SendMessage(update.Message.Chat, $"Guid задачи введен некорректно", cancellationToken: cancellationToken);
        }

        //Вывод отчетности
        private async Task ReportCommand(ToDoUser? user, Update update, string[] text, CancellationToken cancellationToken)
        {
            if (user == null) throw new UserIsNotRegistratedException();//если пользователь не зарегистрирован
            var report = await _toDoReportService.GetUserStats(user.UserId, cancellationToken);
            await _telegramBotClient.SendMessage(update.Message.Chat, $"Статистика по задачам на {report.generatedAt}. Всего: {report.total}; " +
                $"Завершенных: {report.completed}; Активных: {report.active};", cancellationToken: cancellationToken);
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken ct)
        {
            Console.WriteLine($"Ошибка обработчика: {exception})");
            return Task.CompletedTask;
        }

        public static string EscapeMarkdownV2(string text)
        {
            char[] reservedChars = { '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' };
            foreach (var ch in reservedChars)
            {
                text = text.Replace(ch.ToString(), $"\\{ch}");
            }
            return text;
        }
    }
}
