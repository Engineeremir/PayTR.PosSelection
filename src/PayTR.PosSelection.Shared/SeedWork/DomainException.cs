namespace PayTR.PosSelection.Shared.SeedWork
{
    public class DomainException : Exception
    {
        public Dictionary<string, object?> Extensions { get; set; }
        public DomainException() {}
        public DomainException(string message) : base(message) {}
        public DomainException(string message, Exception innerException) : base(message, innerException) {}
        public DomainException(string message, Dictionary<string, object?> extensions) : base(message) 
        {
            Extensions = extensions;
        }
    }
}