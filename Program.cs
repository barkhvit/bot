namespace Bot
{
    internal class Program
    {
        public static string Name = ""; //Имя пользователя
        public static string[] Input; //ввод от пользователя
        public static string[] CommandsBeforeStart = { "/start - запуск программы", 
            "/help - помощь", "/info - информация о программе", "/exit - выход" };//список команд, до команды Start.
        public static string[] CommandsAfterStart = { "/echo аргумент - вывод в консоль",
            "/help - помощь", "/info - информация о программе", "/exit - выход" };//список команд, после команды Start.

        static void Main(string[] args)
        {
            Console.WriteLine("Вас приветствует бот.");
            while(true)
            {
                //вывод приветствия и список доступных команд
                Welcome(Name);

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
                    case "/start":
                        ComandStart();
                        continue;
                    case "/help":
                        ComandHelp();
                        break;
                    case "/info":
                        ComandInfo();
                        break;
                    case "/echo":
                        ComandEcho();
                        break;
                    case "/exit":
                        return;
                    default:
                        WrongInput();
                        break;
                }
                
            }
            //-------------------------------------------------------------
            
            //Вывод приветствия в консоль.
            void Welcome(string name = "")
            {
                if (name == "") // без имени пользователя
                {
                    Console.WriteLine("Введите одну из команд:");
                    PrintCommands(CommandsBeforeStart);
                }

                else //с именем пользователя
                {
                    Console.WriteLine($"{name}, введите одну из команд:");
                    PrintCommands(CommandsAfterStart);
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

        }
    }
}
