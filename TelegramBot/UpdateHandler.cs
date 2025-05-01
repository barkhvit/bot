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
        //private readonly string CommandsForRegUser = "\n /info \n /help \n /addtask \n /removetask \n /completetask \n /showtasks \n /showalltasks \n /find";//список команд для зарегистрированного пользователя

        //события 
        public event MessageEventHandler? OnHandleUpdateStarted;
        public event MessageEventHandler? OnHandleUpdateCompleted;

        //КОНСТРУКТОР
        public UpdateHandler(ITelegramBotClient telegramBotClient, IUserService userService, IToDoService todoService, IToDoReportService toDoReportService)
        {   
            _telegramBotClient = telegramBotClient;
            _userService = userService;
            _todoService = todoService;
            _toDoReportService = toDoReportService;
        }

        //Обработчик команд от пользователя 
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            string[] _text = update.Message.Text.Split(' ',2); //сообщение от пользователя помещаем в двумерный массив
            ToDoUser? _user = await _userService.GetUser(update.Message.From.Id, cancellationToken);//присваиваем пользователя локальной переменной

            // Вызываем событие "Началась обработка"
            OnHandleUpdateStarted?.Invoke(update.Message.Text);

            try
            {
                if (_text.Length == 0) throw new IncorrectInputException();
                
                //обработка команды от пользователя
                switch (_text[0])
                {
                    case "/start": await StartCommand(_user, update, cancellationToken);break;
                    case "/help": await ComandHelp(_user, update, cancellationToken); break;
                    case "/info": await botClient.SendMessage(update.Message.Chat, "| Версия программы: 2.0 | Дата создания: 30.03.2025 | Автор: Бархатов Виталий", cancellationToken: cancellationToken); ; break;
                    case "/addtask": await AddTaskCommand(_user, update, _text, cancellationToken);break;
                    case "/showtasks": await ShowTasksCommand(_user, update, cancellationToken); break;
                    case "/showalltasks": await ShowAllTasksCommandAsync(_user, update, cancellationToken); break;
                    case "/completetask": await CompleteTaskCommand(_user, update, _text, cancellationToken);break;
                    case "/removetask": await RemoveTaskCommand(_user, update, _text, cancellationToken); break;
                    case "/report": await ReportCommand(_user, update, _text, cancellationToken); break;
                    case "/find": await FindCommandAsync(_user, update, _text, cancellationToken); break;
                    case "/showusers": break;
                    case "/exit": break;
                    default: throw new IncorrectInputException(); 
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
            finally
            {
                //событие "конец обработки"
                OnHandleUpdateCompleted?.Invoke(update.Message.Text);
            }
        }

        //=====================================================================================================================================================

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

        //показать все задачи (активные и завершенные для пользователя)
        private async Task ShowAllTasksCommandAsync(ToDoUser? _user, Update update, CancellationToken cancellationToken)
        {

            if (_user == null) throw new UserIsNotRegistratedException();//если пользователь не зарегистрирован
            string str = "Задач нет";
            string str2 = "";
            var toDoItems = await _todoService.GetAllByUserId(_user.UserId, cancellationToken);
            if (toDoItems.Count != 0)
            {
                str = "Список задач:";
                foreach(ToDoItem toDoItem in toDoItems)
                {
                    str2 = $"\n  ({toDoItem.State}) {toDoItem.Name} - {toDoItem.CreatedAt} - \\`{toDoItem.Id}\\`";
                    str = str + str2;
                }
            }
            await _telegramBotClient.SendMessage(update.Message.Chat, EscapeMarkdownV2(str), cancellationToken: cancellationToken, parseMode: ParseMode.MarkdownV2);
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
        
        private async Task ShowTasksCommand(ToDoUser? _user, Update update, CancellationToken cancellationToken)//ВЫВОДИТ активные задачи для пользователя
        {
            if (_user == null) throw new UserIsNotRegistratedException();//если пользователь не зарегистрирован
            var toDoItems = await _todoService.GetActiveByUserId(_user.UserId,cancellationToken);
            string str = "Активных задач нет";
            if (toDoItems.Count != 0)
            {
                str = "Список активных задач:";
                foreach(ToDoItem Item in toDoItems)
                {
                    str = str + $"\n{Item.Name} - {Item.CreatedAt} - \\`{Item.Id}\\`";
                }
            }
            await _telegramBotClient.SendMessage(update.Message.Chat, EscapeMarkdownV2(str), cancellationToken: cancellationToken, parseMode: ParseMode.MarkdownV2);
        }

        private async Task AddTaskCommand(ToDoUser? _user, Update update, string[] _text, CancellationToken cancellationToken)//ДОБАВИТЬ задачу
        {
            ToDoItem toDoItem = await _todoService.Add(_user, _text, cancellationToken);
            await _telegramBotClient.SendMessage(update.Message.Chat, $"Задача \"{toDoItem.Name}\" добавлена, GuidId: `{toDoItem.Id}`", cancellationToken: cancellationToken, parseMode: ParseMode.MarkdownV2);
        }
        
        private async Task ComandHelp(ToDoUser? _user, Update update, CancellationToken cancellationToken)//команда help
        {
            string str = "Для начала работы введите команду /start";
            if (_user != null)
                str = "Доступные команды:\n /addtask название задачи - добавить новую задачу" +
                    "\n /removetask Id - удалить задачу" +
                    " \n /completetask Id - завершить задачу" +
                    " \n /showtasks - показать активные задачи" +
                    " \n /showalltasks - показать все задачи" +
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
