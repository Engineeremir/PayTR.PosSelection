namespace PayTR.PosSelection.Shared.SeedWork
{
    public class UndefinedApplicationException : Exception
    {
        public Dictionary<string, object?> Extensions { get; set; }
        public UndefinedApplicationException() { }
        public UndefinedApplicationException(string message) : base(message) { }
        public UndefinedApplicationException(string message, Exception innerException) : base(message, innerException) { }
    }
}