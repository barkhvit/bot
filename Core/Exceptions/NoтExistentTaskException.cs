namespace Bot.Core.Exceptions
{
    [Serializable]
    internal class NoтExistentTaskException : Exception
    {
        public NoтExistentTaskException()
        {
        }

        public NoтExistentTaskException(string? message) : base(message)
        {
        }

        public NoтExistentTaskException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}