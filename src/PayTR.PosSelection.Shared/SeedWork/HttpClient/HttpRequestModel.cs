using System.Net.Mime;

namespace PayTR.PosSelection.Shared.SeedWork.HttpClient;

public class HttpRequestModel
{
    public string Url { get; set; }
    public Dictionary<string, object> RequestParameters { get; set; }
    public Dictionary<string, object> Headers { get; set; }
    public Dictionary<string, object> PathVariables { get; set; }
    public Dictionary<string, object> QueryParams { get; set; }
    public string ContentType { get; set; } = MediaTypeNames.Application.Json;
    public bool RetryPolicy { get; set; } = false;
}