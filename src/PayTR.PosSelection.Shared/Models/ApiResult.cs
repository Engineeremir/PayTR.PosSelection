using System.Net;

namespace PayTR.PosSelection.Shared.Models;

public class ApiResult<T>
{
    public int StatusCode { get; set; }
    public string Title { get; set; }
    public T? Data { get; set; }
    public ApiProblemDetails Error { get; set; }

    public ApiResult<T> ResponseOk(T data)
    {
        StatusCode = (int)HttpStatusCode.OK;
        Data = data;
        Title = "Success";

        return this;
    }
}
