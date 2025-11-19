namespace PayTR.PosSelection.Shared.SeedWork
{
    public class ApplicationGeneralException : Exception
    {
        public Dictionary<string, object?> Extensions { get; set; }
        public ApplicationGeneralException() { }
        public ApplicationGeneralException(string message) : base(message) { }
        public ApplicationGeneralException(string message, Exception innerException) : base(message, innerException) { }
        public ApplicationGeneralException(string message, Dictionary<string, object?> extensions) : base(message)
        {
            Extensions = extensions;
        }
    }
}
