using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using static System.Net.Mime.MediaTypeNames;

namespace Bot
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly string CommandsForRegUser = "\n /info \n /help \n /addtask \n /removetask \n /completetask \n /showtasks \n /showalltasks";//список команд для зарегистрированного пользователя
        private readonly string CommandsForNotRegUser = "\n /start - начать\n /info - о программе \n /help - помощь";//список команд для незарегистрированного пользователя
        private readonly int TaskLimit = 30;//лимит по длине задачи
        private readonly int ToDoItemsLimit = 5; //лимит по кол-ву задач на пользователя

        //КОНСТРУКТОР
        public UpdateHandler(ITelegramBotClient telegramBotClient, IUserService userService, IToDoService todoService)
        {   
            _telegramBotClient = telegramBotClient;
            _userService = userService;
            _todoService = todoService;
        }

        //Обработчик команд от пользователя 
        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            string[] _text = update.Message.Text.Split(' ',2); //сообщение от пользователя помещаем в двумерный массив
            User? _user = _userService.GetUser(update.Message.From.Id);//присваиваем пользователя локальной переменной
            //ToDoService todoService = (ToDoService)_todoService;

            try
            {
                if (_text.Length == 0)
                {
                    WrongInput(botClient, update);
                    return;
                }
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
                    case "/showusers": break;
                    case "/exit": return;
                    default: WrongInput(botClient,update); break;
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
            catch (IncorrectRemoveTaskException e)//некорректно введен аргумент для команды /removetask
            {
                botClient.SendMessage(update.Message.Chat, $"{e.Message}");
            }
            catch (DuplicateTaskException e)//если дубликат задачи
            {
                botClient.SendMessage(update.Message.Chat, $"{e.Message}");
            }
            catch (TaskCountLimitException e)//превышен лимит кол-ва задач
            {
                botClient.SendMessage(update.Message.Chat, $"{e.Message}");
            }
        }

        //показать все задачи (активные и завершенные для пользователя)
        private void ShowAllTasksCommand(User? _user, Update update)
        {

            if (_user == null)//если пользователь не зарегистрирован
            {
                _telegramBotClient.SendMessage(update.Message.Chat, "для начала работы введите /start");
                return;
            }
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

        private void StartCommand(User? _user, Update update) //команда старт
        {
            // регистрируем пользователя
            _user = _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username);
            _telegramBotClient.SendMessage(update.Message.Chat, $"Доступные команды: {CommandsForRegUser}");
        }
        

        private void ShowTasksCommand(User? _user, Update update)
        {
            if (_user == null)//если пользователь не зарегистрирован
            {
                _telegramBotClient.SendMessage(update.Message.Chat, "для начала работы введите /start");
                return;
            }
            string str = "Активных задач нет";
            var toDoItems = _todoService.GetActiveByUserId(_user.UserId);
            if (toDoItems.Count != 0)
            {
                str = "Список активных задач:";
                foreach(ToDoItem Item in _todoService.GetActiveByUserId(_user.UserId))
                {
                    str = str + $"\n{Item.Name} - {Item.CreatedAt} - {Item.Id}";
                }
            }
            _telegramBotClient.SendMessage(update.Message.Chat, str);
        }

        private void AddTaskCommand(User? _user, Update update, string[] _text)//ДОБАВИТЬ задачу
        {
            if (_user == null)//если пользователь не зарегистрирован
            {
                _telegramBotClient.SendMessage(update.Message.Chat, "для начала работы введите /start");
                return;
            }
            ToDoItem toDoItem = _todoService.Add(_user, _text);
            _telegramBotClient.SendMessage(update.Message.Chat, $"Задача \"{toDoItem.Name}\" добавлена, GuidId: {toDoItem.Id}");
        }
        
        private void ComandHelp(User? _user, Update update)//команда help
        {
            string str = "Для начала работы введите команду /start";
            if (_user != null)
                str = "Доступные команды:\n /addtask название задачи - добавить новую задачу" +
                    "\n /removetask Id - удалить задачу" +
                    " \n /completetask Id - завершить задачу" +
                    " \n /showtasks - показать активные задачи" +
                    " \n /showalltasks - показать все задачи";
            _telegramBotClient.SendMessage(update.Message.Chat, str);
        }

        //некорректный ввод команды
        private void WrongInput(ITelegramBotClient botClient,Update update)
        {
            botClient.SendMessage(update.Message.Chat, "Некорректный ввод команды");
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
            if (String.IsNullOrEmpty(str))//проверяем, что строка не ноль не пустая
                throw new ArgumentException();

            int i = str.Replace(" ", "").Length;
            if (i == 0) throw new ArgumentException();//проверяем что строка не состоит только из пробелов
        }

        //УДАЛИТЬ задачу
        private void RemoveTaskCommand(User? user, Update update, string[] _text)
        {
            if (user == null)//если пользователь не зарегистрирован
            {
                _telegramBotClient.SendMessage(update.Message.Chat, "для начала работы введите /start");
                return;
            }
            if (_text.Length == 1 || _text[1] == "") throw new IncorrectRemoveTaskException();
            ValidateString(_text[1]);

            if (Guid.TryParse(_text[1], out var guid))//проверяем корректность GUID и есть ли задача с таким GUID
            {
                _todoService.Delete(guid, out bool isDelete);
                if(isDelete) _telegramBotClient.SendMessage(update.Message.Chat, $"Задача GuidId: \"{_text[1]}\" удалена ");
                else _telegramBotClient.SendMessage(update.Message.Chat, $"Задачи с GuidId \"{_text[1]}\" не cуществует ");
            }
            else
                _telegramBotClient.SendMessage(update.Message.Chat, $"Задачи с GuidId \"{_text[1]}\" не cуществует ");
        }

        //переводит задачу в статус ВЫПОЛНЕНО
        private void CompleteTaskCommand(User? user, Update update, string[] _text) 
        {
            if (user == null)//если пользователь не зарегистрирован
            {
                _telegramBotClient.SendMessage(update.Message.Chat, "для начала работы введите /start");
                return;
            }
            if (_text.Length == 1 || _text[1] == "") throw new IncorrectRemoveTaskException();
            ValidateString(_text[1]);

            if (Guid.TryParse(_text[1], out var guid) && TaskIsInList(guid))//проверяем корректность GUID и есть ли задача с таким GUID
            {
                _todoService.MarkCompleted(guid, out bool isComplete);
                if(isComplete) _telegramBotClient.SendMessage(update.Message.Chat, $"Задача GuidId: \"{_text[1]}\" выполнена ");
                else _telegramBotClient.SendMessage(update.Message.Chat, $"Задачи с GuidId \"{_text[1]}\" нет ");
            }
            else
                _telegramBotClient.SendMessage(update.Message.Chat, $"Задачи с GuidId \"{_text[1]}\" нет ");
        }

        //проверка по Guid есть такая задача или нет
        public bool TaskIsInList(Guid guid2)
        {
            foreach (ToDoItem toDoItem in _todoService.GetToDoItems())
                if (toDoItem.Id == guid2) return true;
            
            return false;
        }

        //проверка, что пользователь зарегистрирован
    }
}
