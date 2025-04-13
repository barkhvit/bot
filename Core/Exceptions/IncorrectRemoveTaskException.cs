namespace Bot.Core.Exceptions
{
    internal class IncorrectArgumentTaskException : Exception
    {
        public string Type { get; private set; }
        public IncorrectArgumentTaskException(string str)
        {
            Type = str;
        }
    }
}