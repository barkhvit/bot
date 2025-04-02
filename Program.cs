using System.Linq.Expressions;
using static System.Net.Mime.MediaTypeNames;
using Otus.ToDoList.ConsoleBot;
using System.Threading.Channels;

namespace Bot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var botClient = new ConsoleBotClient();//консольный бот, имитирует работу телеграм-бота
            var userService = new UserService();//сервис по работе с пользователями
            var toDoService = new ToDoService();//сервис по работе с листом заданий
            var updateHandler = new UpdateHandler(botClient, userService, toDoService);//обработчик команд

            try
            {
                botClient.StartReceiving(updateHandler);
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nПроизошла непредвиденная ошибка: \n{e.GetType}\n{e.Message}\n{e.InnerException}\n{e.StackTrace}");
            }
        }
    }
}
          
