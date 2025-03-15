namespace Bot
{
    internal class Program
    {
        public static string Name = ""; //Имя пользователя
        public static string[] Input; //ввод от пользователя
        public static string[] CommandsBeforeStart = { "/start - запуск программы", 
            "/help - помощь", "/info - информация о программе", "/exit - выход" };//список команд, до команды Start.

        public static string[] CommandsAfterStart = { "/echo аргумент - вывод в консоль",
            "/help - помощь", "/info - информация о программе", "/exit - выход",
            "/addtask - добавить задачу", "/showtasks - показать список задач",
            "/removetask - удалить задачу","/clear - очистить экран"};//список команд, после команды Start.

        public static List<string> Tasks = new List<string>(); // список задач
        public static bool ProgramIsStarting = false; // введена команда Start, программа запущена

        static void Main(string[] args)
        {
            Console.WriteLine("Вас приветствует бот.");//приветствие
            while(true)
            {
                //вывод приветствия и список доступных команд
                if(!ProgramIsStarting) Welcome(Name);

                //получение команды от пользователя
                string input = Console.ReadLine();
                if (input != null)
                {
                    Input = input.Split(" ",2);
                }
                else
                {
                    WrongInput();
                    continue;
                }

                //обработка команды от пользователя
                switch (Input[0])
                {
                    case "/start":ComandStart();continue;
                    case "/help":ComandHelp();break;
                    case "/info":ComandInfo();break;
                    case "/echo":ComandEcho();break;
                    case "/addtask":ComandAddTask();break;
                    case "/showtasks": ComandShowTasks(); break;
                    case "/removetask": ComandRemoveTasks(); break;
                    case "/clear": ComandClear(); break;
                    case "/exit":return;
                    default:WrongInput();break;
                }
            }
            //-------------------------------------------------------------
            
            //Вывод приветствия в консоль.
            void Welcome(string name = "")
            {
                if (name == "") // без имени пользователя
                {
                    Console.WriteLine("Доступные команды:");
                    PrintCommands(CommandsBeforeStart);
                }

                else //с именем пользователя
                {
                    Console.WriteLine($"{name}, чем я могу Вам помочь? Доступные команды:");
                    Console.WriteLine();
                    PrintCommands(CommandsAfterStart);
                    ProgramIsStarting = true;
                }
            }

            //Вывод списка команд в консоль.
            void PrintCommands(string[] commands)
            {
                foreach(string c in commands)
                {
                    Console.WriteLine(c);
                }
                Console.WriteLine("-----------------------");
            }

            //Команда /start
            void ComandStart()
            {
                if (Name == "")
                {
                    Console.WriteLine("");
                    Console.WriteLine("Введите Ваше имя:");
                    Name = Console.ReadLine();
                    Console.Clear();
                }
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine("Программа уже запущена");
                    Console.WriteLine("");
                }

            }

            //Команда /echo
            void ComandEcho()
            {
                if (Name == "")
                {
                    WrongInput();
                }
                else if(Input.Length<2)
                {
                    WrongInputEcho();
                }
                else
                {
                    Console.WriteLine(Input[1]);
                    Console.WriteLine("");
                }
                    
            }

            //Команда /help
            void ComandHelp()
            {
                Console.WriteLine("");
                Console.WriteLine("Для старта программы введите команду: /start \nЕсли программа запущена то введите команду /echo и через пробел аргумент");
                Console.WriteLine("");
            }

            //Команда /info
            void ComandInfo()
            {
                Console.WriteLine("");
                Console.WriteLine("| Версия программы: 1.0 | Дата создания: 12.03.2025 |");
                Console.WriteLine("");
            }

            //неверный ввод команды
            void WrongInput()
            {
                Console.WriteLine("");
                Console.WriteLine("Команда введена неверно");
                Console.WriteLine("");
            }

            //неверный ввод команды Echo
            void WrongInputEcho()
            {
                Console.WriteLine("");
                Console.WriteLine("Ошибка ввода. После команды /echo через пробел необходимо ввести аргумент");
                Console.WriteLine("");
            }

            //неверный ввод команды Echo
            void WrongInputAddtask()
            {
                Console.WriteLine("");
                Console.WriteLine("Для начала работы введите команду /start");
                Console.WriteLine("");
            }

            //команда добавить задачу
            void ComandAddTask()
            {
                if (Name!="")
                {
                    Console.Write("Пожалуйста, введите описание задачи:");
                    string InputTask = Console.ReadLine();
                    if (InputTask != "")
                    {
                        Tasks.Add(InputTask);
                        Console.WriteLine($"Задача \"{InputTask}\" добавлена.");
                        Console.WriteLine();
                    }
                }
                else
                {
                    WrongInputAddtask();
                }
            }

            //команда показать задачи
            void ComandShowTasks()
            {
                if (Name != "")
                {
                    if (Tasks.Count > 0)
                    {
                        Console.WriteLine("Список задач:");
                        ShowTasks();
                    }
                    else
                    {
                        Console.WriteLine("Задач нет. Добавьте задачу с помощью команды /addtask");
                        Console.WriteLine("");
                    }
                }
                else
                {
                    WrongInputAddtask();
                }
            }

            //вывод задач
            void ShowTasks()
            {
                for(int n = 0; n < Tasks.Count; n++)
                {
                    Console.WriteLine($"{n+1}) {Tasks[n]}");
                }
                Console.WriteLine("");
            }

            //команда удалить задачу
            void ComandRemoveTasks()
            {
                if (Name != "")//проверяем, что программа запущена
                {
                    if (Tasks.Count > 0)
                    {
                        Console.WriteLine("Вот список задач:");
                        ShowTasks();
                        Console.Write("Введите номер задачи, которую вы хотите удалить:");
                        try
                        {
                            int n;
                            if(int.TryParse(Console.ReadLine(), out n))
                            {
                                string RemoveTask = Tasks[n - 1];
                                Tasks.RemoveAt(n-1);
                                Console.WriteLine($"Задача {RemoveTask} удалена.");
                            }
                            else
                            {
                                Console.WriteLine("Некорректно введен номер задачи.");
                                Console.WriteLine("");
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Некорректно введен номер задачи.");
                            Console.WriteLine("");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Задач нет. Добавьте задачу с помощью команды /addtask");
                        Console.WriteLine("");
                    }
                }
                else
                {
                    WrongInputAddtask();
                }
            }

            //команда очистить экран
            void ComandClear()
            {
                Console.Clear();
                Welcome(Name);
            }
        }
    }
}
