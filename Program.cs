using System.Linq.Expressions;
using static System.Net.Mime.MediaTypeNames;
//using Otus.ToDoList.ConsoleBot;
using System.Threading.Channels;
using Bot.Core.Services;
using Bot.TelegramBot;
using Bot.Infrastructure.DataAccess;
using Telegram.Bot;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Bot.TelegramBot.Scenarios;
using Bot.Core.DataAccess;
using Bot.BackgroundTask;

namespace Bot
{
    //HR Bot
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //бот-клиент
            //var botClient = new ConsoleBotClient();//консольный бот, имитирует работу телеграм-бота
            string Token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.User);
            var botClient = new TelegramBotClient(Token);

            var connectionString = "User ID=postgres;Password=Alekseev4+;Host=localhost;Port=5432;Database=ToDoList;";

            // Создаем фабрику 
            IDataContextFactory<ToDoDataContext> contextFactory = new DataContextFactory(connectionString);

            //репозитории
            //var userRepository = new FileUserRepository("usersRepository");
            //var toDoRepository = new FileToDoRepository("toDoRepository");
            //var todoListRepository = new FileToDoListRepository("ToDoLists");
            var userRepository = new SqlUserRepository(contextFactory);
            var toDoRepository = new SqlToDoRepository(contextFactory);
            var todoListRepository = new SqlToDoListRepository(contextFactory);
            var contextRepository = new InMemoryScenarioContextRepository();
            

            //сервисы
            var userService = new UserService(userRepository);//сервис по работе с пользователями
            var toDoService = new ToDoService(30, toDoRepository);//сервис по работе с листом заданий
            var reportService = new ToDoReportService(toDoRepository); // сервис по работе с отчетностью
            var toDoListService = new ToDoListService(todoListRepository);

            //сценарии
            var scenarios = new List<IScenario>
            {
                new AddTaskScenario(userService,toDoService, toDoListService),
                new AddListScenario(userService,toDoListService, contextRepository),
                new DeleteListScenario(userService,toDoListService, contextRepository,toDoService)
            };

            //обработчик
            var callBackUpdateHandler = new CallBackUpdateHandler(botClient, userService, toDoService, reportService, scenarios, contextRepository, toDoListService);
            var updateHandler = new UpdateHandler(botClient, userService, toDoService, reportService, scenarios, contextRepository, toDoListService, callBackUpdateHandler);//обработчик команд
            

            //методы событий начала и окончания обработки
            updateHandler.OnHandleUpdateStarted += text => Console.WriteLine($"Началась обработка сообщения:'{text}'");
            updateHandler.OnHandleUpdateCompleted += text => Console.WriteLine($"Закончилась обработка сообщения:'{text}'");

            //CancellationToken
            var _cts = new CancellationTokenSource();

            // Запускаем фоновую задачу для обработки нажатия клавиши
            var keyMonitorTask = Task.Run(() => MonitorKeyboard(botClient, _cts));

            //1. Создаем runner для фоновых задач
            var backgroundTaskRunner = new BackgroundTaskRunner();

            //2. Регистрируем задачу сброса сценариев
            //    Таймаут - 1 час, передаем репозиторий и клиент бота
            backgroundTaskRunner.AddTask(new ResetScenarioBackgroundTask(
                TimeSpan.FromHours(1), contextRepository,
                botClient));

            //3. Запускаем фоновые задачи
            //    Передаем токен отмены, чтобы задачи могли остановиться при завершении приложения
            backgroundTaskRunner.StartTasks(_cts.Token);

            try
            {
                //botClient.StartReceiving(updateHandler, _cts);
                botClient.StartReceiving(
                    updateHandler: updateHandler.HandleUpdateAsync,
                    errorHandler: updateHandler.HandleErrorAsync,
                    cancellationToken: _cts.Token
                    );

                // Выводим информацию о боте
                var me = await botClient.GetMe();
                Console.WriteLine($"Бот @{me.Username} запущен. Нажмите клавишу A для выхода.");
                await Task.Delay(-1, _cts.Token);
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nПроизошла непредвиденная ошибка: \n{e.GetType}\n{e.Message}\n{e.InnerException}\n{e.StackTrace}");
            }
            finally
            {
                // При завершении приложения:
                // Останавливаем фоновые задачи
                await backgroundTaskRunner.StopTasks(CancellationToken.None);

                // Освобождаем ресурсы
                backgroundTaskRunner.Dispose();

                if (updateHandler != null)
                {
                    //отписка от событий
                    updateHandler.OnHandleUpdateStarted -= text => Console.WriteLine($"Началась обработка сообщения:'{text}'");
                    updateHandler.OnHandleUpdateCompleted -= text => Console.WriteLine($"Закончилась обработка сообщения:'{text}'");
                }
            }
        }

        private static void MonitorKeyboard(TelegramBotClient botClient, CancellationTokenSource _cts)
        {
            while (!_cts.IsCancellationRequested)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.A)
                {
                    Console.WriteLine("\nОбнаружено нажатие клавиши A. Завершение работы...");
                    _cts.Cancel();
                    break;
                }
                else
                {
                    // Если нужно, можно вывести информацию о боте при нажатии любой другой клавиши
                    Task.Run(async () =>
                    {
                        var me = await botClient.GetMe();
                        Console.WriteLine($"\nИнформация о боте: @{me.Username}, ID: {me.Id}, Имя: {me.FirstName}");
                    }).Wait(); // Wait используется здесь только для демонстрации
                }
            }
        }
    }
}
          
