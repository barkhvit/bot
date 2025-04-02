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
        ConsoleBotClient _telegramBotClient;
        IUserService _userService;
        ToDoService _todoService;
        User? _user; //пользователь
        Update _update; //обновление от пользователя
        string[] _text; //текст от пользователя
        bool isRegisteredUser = false; //зарегистрирован пользователь или нет
        string CommandsForRegUser = "\n /info \n /help \n /addtask \n /removetask \n /completetask \n /showtasks";//список команд для зарегистрированного пользователя
        string CommandsForNotRegUser = "\n /start - начать\n /info - о программе \n /help - помощь";//список команд для незарегистрированного пользователя
        const int TaskLimit = 30;//лимит по длине задачи

        //КОНСТРУКТОР
        public UpdateHandler(ConsoleBotClient telegramBotClient, IUserService userService, ToDoService todoService)
        {   
            _telegramBotClient = telegramBotClient;
            _userService = userService;
            _todoService = todoService;
        }

        //Обработчик команд от пользователя 
        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            _update = update; //присваиваем входящий update локальной переменной
            _text = update.Message.Text.Split(' ',2); //сообщение от пользователя помещаем в двумерный массив
            //проверяем зарегистрирован или нет пользователь
            _user = _userService.GetUser(update.Message.From.Id);//присваиваем пользователя локальной переменной
            bool isRegistered = _user != null; //зарегистрирован пользователь или нет

            try
            {
                //обработка команды от пользователя
                switch (_text[0])
                {
                    case "/start": StartCommand();break;
                    case "/help": ComandHelp(); break;
                    case "/info": botClient.SendMessage(update.Message.Chat, "| Версия программы: 2.0 | Дата создания: 30.03.2025 | Автор: Бархатов Виталий"); ; break;
                    case "/addtask": AddTaskCommand();break;
                    case "/showtasks": ShowTasksCommand(); break;
                    case "/completetask": CompleteTaskCommand();break;
                    case "/removetask": RemoveTaskCommand(); break;
                    case "/showusers": break;
                    case "/exit": return;
                    default: WrongInput(botClient,update); break;
                }
            }
            catch (ArgumentException e) //некорректный аргумент при вводе кол-ва задач и символов в задаче или задача == null
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

        

        private void StartCommand() //команда старт
        {
            // регистрируем пользователя
            _user = _userService.RegisterUser(_update.Message.From.Id, _update.Message.From.Username);
            isRegisteredUser = true;
            _telegramBotClient.SendMessage(_update.Message.Chat, $"Доступные команды: {CommandsForRegUser}");
        }
        

        private void ShowTasksCommand()
        {
            string str = "Активных задач нет";
            IReadOnlyList<ToDoItem> toDoItems = new List<ToDoItem>();
            toDoItems = _todoService.GetActiveByUserId(_user.UserId);
            if (toDoItems.Count != 0)
            {
                str = "Список задач:";
                foreach(ToDoItem Item in toDoItems)
                {
                    str = str + $"\n{Item.Name} - {Item.CreatedAt} - {Item.Id}";
                }
            }
            _telegramBotClient.SendMessage(_update.Message.Chat, str);
        }

        private void AddTaskCommand()//ДОБАВИТЬ задачу
        {
            if (!isRegisteredUser)//если пользователь не зарегистрирован
            {
                _telegramBotClient.SendMessage(_update.Message.Chat, "для начала работы введите /start");
                return;
            }
            if(_text.Length == 1 || _text[1] == "") throw new IncorrectTaskException();
            ValidateString(_text[1]);
            _telegramBotClient.SendMessage(_update.Message.Chat, $"Задача \"{_todoService.Add(_user, _text[1]).Name}\" добавлена, GuidId: {_todoService.Add(_user, _text[1]).Id}");
        }
        
        private void ComandHelp()//команда help
        {
            string str = "Для начала работы введите команду /start";
            if (isRegisteredUser)
                str = "Доступные команды:\n /addtask \n /removetask \n /completetask \n /showtasks";
            _telegramBotClient.SendMessage(_update.Message.Chat, str);
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

        //проверяет, что строка не равна null, не равна пустой строке и имеет какие-то символы кроме проблема
        public static void ValidateString(string? str)
        {
            if (String.IsNullOrEmpty(str))//проверяем, что строка не ноль не пустая
                throw new ArgumentException();

            int i = str.Replace(" ", "").Length;
            if (i == 0) throw new ArgumentException();//проверяем что строка не состоит только из пробелов
        }

        //УДАЛИТЬ задачу
        private void RemoveTaskCommand()
        {
            if (!isRegisteredUser)//если пользователь не зарегистрирован
            {
                _telegramBotClient.SendMessage(_update.Message.Chat, "для начала работы введите /start");
                return;
            }
            if (_text.Length == 1 || _text[1] == "") throw new IncorrectRemoveTaskException();
            ValidateString(_text[1]);

            if (Guid.TryParse(_text[1], out var guid) && TaskIsInList(guid))//проверяем корректность GUID и есть ли задача с таким GUID
            {
                _todoService.Delete(guid);
                _telegramBotClient.SendMessage(_update.Message.Chat, $"Задача GuidId: \"{_text[1]}\" удалена ");
            }
            else
                _telegramBotClient.SendMessage(_update.Message.Chat, $"Задачи с GuidId \"{_text[1]}\" не cуществует ");
        }

        //переводит задачу в статус ВЫПОЛНЕНО
        private void CompleteTaskCommand() 
        {
            if (!isRegisteredUser)//если пользователь не зарегистрирован
            {
                _telegramBotClient.SendMessage(_update.Message.Chat, "для начала работы введите /start");
                return;
            }
            if (_text.Length == 1 || _text[1] == "") throw new IncorrectRemoveTaskException();
            ValidateString(_text[1]);

            if (Guid.TryParse(_text[1], out var guid) && TaskIsInList(guid))//проверяем корректность GUID и есть ли задача с таким GUID
            {
                _todoService.MarkCompleted(guid);
                _telegramBotClient.SendMessage(_update.Message.Chat, $"Задача GuidId: \"{_text[1]}\" выполнена ");
            }
            else
                _telegramBotClient.SendMessage(_update.Message.Chat, $"Задачи с GuidId \"{_text[1]}\" нет ");
        }

        //проверка по Guid есть такая задача или нет
        public bool TaskIsInList(Guid guid2)
        {
            foreach (ToDoItem toDoItem in _todoService.GetToDoItems())
                if (toDoItem.Id == guid2) return true;
            
            return false;
        }


    }
}
