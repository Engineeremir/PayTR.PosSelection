namespace PayTR.PosSelection.Shared.Models;

public enum ExceptionType
{
    ValidationException,
    ApplicationException,
    DomainException,
    DbException,
    UnhandledException,
    MappingException,
    UnauthorizedAccessException,
    AuthenticationException
}