using Microsoft.Extensions.Logging;

namespace PayTR.PosSelection.Shared.SeedWork.HttpClient;

public class BaseHttpClientService(IHttpClientFactory httpClientFactory, ILogger<HttpClientService> logger) 
    : HttpClientService(httpClientFactory, logger), IBaseHttpClientService
{
}