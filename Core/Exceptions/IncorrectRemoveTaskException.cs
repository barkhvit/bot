namespace Bot.Core.Exceptions
{
    internal class IncorrectRemoveTaskException : Exception
    {
        public IncorrectRemoveTaskException() : base("После команд /removetask и /completetask необходимо через пробел ввести GuidId задачи")
        {
        }

    }
}