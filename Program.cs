using System.Linq.Expressions;
using static System.Net.Mime.MediaTypeNames;
using Otus.ToDoList.ConsoleBot;
using System.Threading.Channels;
using Bot.Core.Services;
using Bot.TelegramBot;
using Bot.Infrastructure.DataAccess;

namespace Bot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //бот-клиент
            var botClient = new ConsoleBotClient();//консольный бот, имитирует работу телеграм-бота

            //репозитории
            var userRepository = new InMemoryUserRepository();
            var toDoRepository = new InMemoryToDoRepository();

            //сервисы
            var userService = new UserService(userRepository);//сервис по работе с пользователями
            var toDoService = new ToDoService(30, toDoRepository);//сервис по работе с листом заданий
            var reportService = new ToDoReportService(toDoRepository); // сервис по работе с отчетностью

            //обработчик
            var updateHandler = new UpdateHandler(botClient, userService, toDoService, reportService);//обработчик команд

            //методы событий начала и окончания обработки
            updateHandler.OnHandleUpdateStarted += text => Console.WriteLine($"Началась обработка сообщения:'{text}'");
            updateHandler.OnHandleUpdateCompleted += text => Console.WriteLine($"Закончилась обработка сообщения:'{text}'");

            //CancellationToken
            var _cts = new CancellationTokenSource().Token;

            try
            {
                botClient.StartReceiving(updateHandler, _cts);
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nПроизошла непредвиденная ошибка: \n{e.GetType}\n{e.Message}\n{e.InnerException}\n{e.StackTrace}");
            }
            finally
            {
                if (updateHandler != null)
                {
                    //отписка
                    updateHandler.OnHandleUpdateStarted -= text => Console.WriteLine($"Началась обработка сообщения:'{text}'");
                    updateHandler.OnHandleUpdateCompleted -= text => Console.WriteLine($"Закончилась обработка сообщения:'{text}'");
                }
            }
        }
    }
}
          
