using System.Linq.Expressions;
using static System.Net.Mime.MediaTypeNames;
using Otus.ToDoList.ConsoleBot;
using System.Threading.Channels;

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
        public static int taskCountLimit;//лимит по кол-ву задач
        public static int taskLengthLimit;//лимит по кол-ву символов в задаче
        public static List<string> Tasks = new List<string>(); // список задач
        public static bool ProgramIsStarting = false; // введена команда Start, программа запущена

        
        static void Main(string[] args)
        {
            Console.WriteLine("Вас приветствует бот.");//приветствие
            
            while(true)
            {
                try
                {
                    //вывод приветствия и список доступных команд
                    if (!ProgramIsStarting) Welcome(Name);

                    //получение команды от пользователя
                    string input = Console.ReadLine();
                    if (input != null)
                    {
                        Input = input.Split(" ", 2);
                    }
                    else
                    {
                        WrongInput();
                        continue;
                    }

                    //обработка команды от пользователя
                    switch (Input[0])
                    {
                        case "/start": ComandStart(); continue;
                        case "/help": ComandHelp(); break;
                        case "/info": ComandInfo(); break;
                        case "/echo": ComandEcho(); break;
                        case "/addtask": ComandAddTask(); break;
                        case "/showtasks": ComandShowTasks(); break;
                        case "/removetask": ComandRemoveTasks(); break;
                        case "/clear": ComandClear(); break;
                        case "/exit": return;
                        default: WrongInput(); break;
                    }
                }
                catch(ArgumentException e) //некорректный аргумент при вводе кол-ва задач и символов в задаче или задача == null
                {
                    Console.WriteLine("Некорректный ввод.");
                    Name = "";
                    continue;
                }
                catch(TaskLengthLimitException e)//превышен лимит по символам в задаче
                {
                    Console.WriteLine($"{e.Message}\n");
                    continue;
                }
                catch(DuplicateTaskException e)//если дубликат задачи
                {
                    Console.WriteLine($"{e.Message}\n");
                    continue;
                }
                catch(TaskCountLimitException e)//превышен лимит кол-ва задач
                {
                    Console.WriteLine($"{e.Message}\n");
                    continue;
                }
                catch(Exception e)
                {
                    Console.WriteLine($"\nПроизошла непредвиденная ошибка: \n{e.GetType}\n{e.Message}\n{e.InnerException}\n{e.StackTrace}");
                }


            }
            //-------------------------------------------------------------
            
            //Вывод приветствия в консоль.
            void Welcome(string name = "")
            {
                if (name == "") // без имени пользователя
                {
                    ProgramIsStarting = false;
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
                    Console.Write("\nВведите Ваше имя:");
                    Name = Console.ReadLine();
                    Console.Write("Введите максимально допустимое количество задач:");
                    taskCountLimit = ParseAndValidateInt(Console.ReadLine(), 1, 100);
                    Console.Write("Введите максимально допустимую длину задачи:");
                    taskLengthLimit = ParseAndValidateInt(Console.ReadLine(), 1, 100);
                    Console.Clear();
                }
                else
                {
                    Console.WriteLine("\nПрограмма уже запущена\n");
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
                    Console.WriteLine($"{Input[1]}\n");
                }
                    
            }

            //Команда /help
            void ComandHelp()
            {
                Console.WriteLine("\nДля старта программы введите команду: /start \nЕсли программа запущена то введите команду /echo и через пробел аргумент\n");
            }

            //Команда /info
            void ComandInfo()
            {
                Console.WriteLine("\n| Версия программы: 1.0 | Дата создания: 12.03.2025 |\n");
            }

            //неверный ввод команды
            void WrongInput()
            {
                Console.WriteLine("\nКоманда введена неверно\n");
            }

            //неверный ввод команды Echo
            void WrongInputEcho()
            {
                Console.WriteLine("\nОшибка ввода. После команды /echo через пробел необходимо ввести аргумент\n");
            }

            //неверный ввод команды Echo
            void WrongInputAddtask()
            {
                Console.WriteLine("\nДля начала работы введите команду /start\n");
            }

            //команда добавить задачу
            void ComandAddTask()
            {
                if (Name!="")//проверяем, что программа запущена
                {
                    //проверяем, что можно добавить задачу
                    if (Tasks.Count >= taskCountLimit)
                        throw new TaskCountLimitException(taskCountLimit);

                    Console.Write("Пожалуйста, введите описание задачи:");
                    string InputTask = Console.ReadLine();

                    //проверяем задачу на корректность (длина, не null, не пустая)
                    ValidateString(InputTask);

                    //проверяем, что длина не превышает лимит
                    if (InputTask.Length > taskLengthLimit)
                        throw new TaskLengthLimitException(InputTask.Length, taskLengthLimit);

                    //проверяем задачу на дубликат
                    if(Tasks.Contains(InputTask))
                        throw new DuplicateTaskException(InputTask);

                    if (InputTask != "")
                    {
                        Tasks.Add(InputTask);
                        Console.WriteLine($"Задача \"{InputTask}\" добавлена.\n");
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
                        Console.WriteLine("Задач нет. Добавьте задачу с помощью команды /addtask\n");
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
                                Console.WriteLine("Некорректно введен номер задачи.\n");
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Некорректно введен номер задачи.\n");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Задач нет. Добавьте задачу с помощью команды /addtask\n");
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

            //приводит строку к int и проверяет, что оно находится в диапазоне min и max
            int ParseAndValidateInt(string? str, int min, int max)
            {
                if(int.TryParse(str, out int result))
                {
                    if (result <= max && result >= min)
                        return result;
                }
                throw new ArgumentException();
            }

            //проверяет, что строка не равна null, не равна пустой строке и имеет какие-то символы кроме проблема
            void ValidateString(string? str)
            {
                if (String.IsNullOrEmpty(str))//проверяем, что строка не ноль не пустая
                    throw new ArgumentException();

                int i = str.Replace(" ", "").Length;
                if(i==0) throw new ArgumentException();//проверяем что строка не состоит только из пробелов
            }

            
        }
    }
}
