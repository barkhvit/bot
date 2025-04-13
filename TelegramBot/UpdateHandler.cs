using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bot.Core.Entities;
using Bot.Core.Exceptions;
using Bot.Core.Services;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using static System.Net.Mime.MediaTypeNames;

namespace Bot.TelegramBot
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoReportService _toDoReportService;
        private readonly string CommandsForRegUser = "\n /info \n /help \n /addtask \n /removetask \n /completetask \n /showtasks \n /showalltasks \n /find";//список команд для зарегистрированного пользователя
        
        //КОНСТРУКТОР
        public UpdateHandler(ITelegramBotClient telegramBotClient, IUserService userService, IToDoService todoService, IToDoReportService toDoReportService)
        {   
            _telegramBotClient = telegramBotClient;
            _userService = userService;
            _todoService = todoService;
            _toDoReportService = toDoReportService;
        }

        //Обработчик команд от пользователя 
        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            string[] _text = update.Message.Text.Split(' ',2); //сообщение от пользователя помещаем в двумерный массив
            ToDoUser? _user = _userService.GetUser(update.Message.From.Id);//присваиваем пользователя локальной переменной
            //ToDoService todoService = (ToDoService)_todoService;

            try
            {
                if (_text.Length == 0) throw new IncorrectInputException();
                
                //обработка команды от пользователя
                switch (_text[0])
                {
                    case "/start": StartCommand(_user, update);break;
                    case "/help": ComandHelp(_user, update); break;
                    case "/info": botClient.SendMessage(update.Message.Chat, "| Версия программы: 2.0 | Дата создания: 30.03.2025 | Автор: Бархатов Виталий"); ; break;
                    case "/addtask": AddTaskCommand(_user, update, _text);break;
                    case "/showtasks": ShowTasksCommand(_user, update); break;
                    case "/showalltasks": ShowAllTasksCommand(_user,update); break;
                    case "/completetask": CompleteTaskCommand(_user, update, _text);break;
                    case "/removetask": RemoveTaskCommand(_user, update, _text); break;
                    case "/report": ReportCommand(_user, update, _text); break;
                    case "/find": FindCommand(_user, update, _text); break;
                    case "/showusers": break;
                    case "/exit": return;
                    default: throw new IncorrectInputException(); 
                }
            }
            catch (ArgumentException) //некорректный аргумент при вводе кол-ва задач и символов в задаче или задача == null
            {
                botClient.SendMessage(update.Message.Chat, "Некорректный ввод.");
            }
            catch (IncorrectTaskException e) //некорректный ввод после команды /addtask
            {
                botClient.SendMessage(update.Message.Chat, $"{e.Message}");
            }
            catch (TaskLengthLimitException e)//превышен лимит по символам в задаче
            {
                botClient.SendMessage(update.Message.Chat, $"{e.Message}");
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
                botClient.SendMessage(update.Message.Chat, $"Ошибка ввода. После команды {e.Type} через пробел необходимо ввести {message}.");
            }
            catch (DuplicateTaskException e)//если дубликат задачи
            {
                botClient.SendMessage(update.Message.Chat, $"{e.Message}");
            }
            catch (TaskCountLimitException e)//превышен лимит кол-ва задач
            {
                botClient.SendMessage(update.Message.Chat, $"{e.Message}");
            }
            catch (UserIsNotRegistratedException)
            {
                botClient.SendMessage(update.Message.Chat, $"для начала работы введите /start");
            }
            catch (NoтExistentTaskException)
            {
                botClient.SendMessage(update.Message.Chat, $"Нет задачи с таким GuidId");
            }
            catch (IncorrectInputException)
            {
                botClient.SendMessage(update.Message.Chat, $"Некорректный ввод команды");
            }
        }

        //================================================================================================

        //поиск задачи 
        private void FindCommand(ToDoUser? user, Update update, string[] text)
        {
            if (user == null) throw new UserIsNotRegistratedException();//если пользователь не зарегистрирован
            if (text.Length == 1 || text[1] == "") throw new IncorrectArgumentTaskException("/find");
            ValidateString(text[1]);
            var itemsResult = _todoService.Find(user, text[1]);
            string str = "Таких задач нет";
            if (itemsResult.Count != 0)
            {
                str = "Результат поиска:";
                foreach (ToDoItem Item in itemsResult)
                {
                    str = str + $"\n({Item.State}) {Item.Name} - {Item.CreatedAt} - {Item.Id}";
                }
            }
            _telegramBotClient.SendMessage(update.Message.Chat, str);
        }


        //показать все задачи (активные и завершенные для пользователя)
        private void ShowAllTasksCommand(ToDoUser? _user, Update update)
        {

            if (_user == null) throw new UserIsNotRegistratedException();//если пользователь не зарегистрирован
            string str = "Задач нет";
            string str2 = "";
            var toDoItems = _todoService.GetAllByUserId(_user.UserId);
            if (toDoItems.Count != 0)
            {
                str = "Список задач:";
                foreach(ToDoItem toDoItem in toDoItems)
                {
                    str2 = $"\n  ({toDoItem.State}) {toDoItem.Name} - {toDoItem.CreatedAt} - {toDoItem.Id}";
                    str = str + str2;
                }
            }
            _telegramBotClient.SendMessage(update.Message.Chat, str);
        }

        private void StartCommand(ToDoUser? _user, Update update) //команда старт
        {
            // регистрируем пользователя
            _user = _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username);
            _telegramBotClient.SendMessage(update.Message.Chat, $"Доступные команды: {CommandsForRegUser}");
        }
        

        private void ShowTasksCommand(ToDoUser? _user, Update update)//ВЫВОДИТ активные задачи для пользователя
        {
            if (_user == null) throw new UserIsNotRegistratedException();//если пользователь не зарегистрирован
            var toDoItems = _todoService.GetActiveByUserId(_user.UserId);
            string str = "Активных задач нет";
            if (toDoItems.Count != 0)
            {
                str = "Список активных задач:";
                foreach(ToDoItem Item in toDoItems)
                {
                    str = str + $"\n{Item.Name} - {Item.CreatedAt} - {Item.Id}";
                }
            }
            _telegramBotClient.SendMessage(update.Message.Chat, str);
        }

        private void AddTaskCommand(ToDoUser? _user, Update update, string[] _text)//ДОБАВИТЬ задачу
        {
            ToDoItem toDoItem = _todoService.Add(_user, _text);
            _telegramBotClient.SendMessage(update.Message.Chat, $"Задача \"{toDoItem.Name}\" добавлена, GuidId: {toDoItem.Id}");
        }
        
        private void ComandHelp(ToDoUser? _user, Update update)//команда help
        {
            string str = "Для начала работы введите команду /start";
            if (_user != null)
                str = "Доступные команды:\n /addtask название задачи - добавить новую задачу" +
                    "\n /removetask Id - удалить задачу" +
                    " \n /completetask Id - завершить задачу" +
                    " \n /showtasks - показать активные задачи" +
                    " \n /showalltasks - показать все задачи" +
                    " \n /find + начало задачи - поиск задач по началу";
            _telegramBotClient.SendMessage(update.Message.Chat, str);
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
        private void RemoveTaskCommand(ToDoUser? user, Update update, string[] _text)
        {
            if (user == null) throw new UserIsNotRegistratedException();//если пользователь не зарегистрирован
            if (_text.Length == 1 || _text[1] == "") throw new IncorrectArgumentTaskException("/removetask");
            ValidateString(_text[1]);

            if (Guid.TryParse(_text[1], out var guid))//проверяем корректность GUID и есть ли задача с таким GUID
            {
                _todoService.Delete(guid);
                _telegramBotClient.SendMessage(update.Message.Chat, $"Задача GuidId: \"{_text[1]}\" удалена ");
            }
            else
                _telegramBotClient.SendMessage(update.Message.Chat, $"Задачи с GuidId \"{_text[1]}\" не cуществует ");
        }

        //переводит задачу в статус ВЫПОЛНЕНО
        private void CompleteTaskCommand(ToDoUser? user, Update update, string[] _text) 
        {
            if (user == null) throw new UserIsNotRegistratedException();//если пользователь не зарегистрирован
            if (_text.Length == 1 || _text[1] == "") throw new IncorrectArgumentTaskException("/completetask");
            ValidateString(_text[1]);

            if (Guid.TryParse(_text[1], out var guid))//проверяем корректность GUID
            {
                _todoService.MarkCompleted(guid, user.UserId);
                _telegramBotClient.SendMessage(update.Message.Chat, $"Задача GuidId: \"{_text[1]}\" выполнена ");
            }
            else
                _telegramBotClient.SendMessage(update.Message.Chat, $"Guid задачи введен некорректно");
        }

        //Вывод отчетности
        private void ReportCommand(ToDoUser? user, Update update, string[] text)
        {
            if (user == null) throw new UserIsNotRegistratedException();//если пользователь не зарегистрирован
            var report = _toDoReportService.GetUserStats(user.UserId);
            _telegramBotClient.SendMessage(update.Message.Chat, $"Статистика по задачам на {report.generatedAt}. Всего: {report.total}; " +
                $"Завершенных: {report.completed}; Активных: {report.active};");
        }

    }
}
