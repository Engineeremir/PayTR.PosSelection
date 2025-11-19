using Microsoft.Extensions.Logging;
using Polly;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace PayTR.PosSelection.Shared.SeedWork.HttpClient;

public abstract class HttpClientService
{
    private readonly IAsyncPolicy<HttpResponseMessage> _defaultRetryPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _noRetryPolicy;
    private readonly IHttpClientFactory _httpClientFactory;

    protected HttpClientService(
        IHttpClientFactory httpClientFactory,
        ILogger<HttpClientService> logger)
    {
        _httpClientFactory = httpClientFactory;
        var retryCount = 5;
        _defaultRetryPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r =>
            r.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
            r.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
            (int)r.StatusCode > 500)
            .WaitAndRetryAsync(
            retryCount,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, currentRetryCount, context) =>
            {
                logger.LogWarning($"Retry {currentRetryCount} after {timespan.TotalSeconds} seconds due to {(outcome.Result != null ? outcome.Result.StatusCode.ToString() : outcome.Exception.Message)}");

                if (currentRetryCount == retryCount)
                {
                    throw new ApplicationException(JsonSerializer.Serialize(outcome));
                }
            });
        _noRetryPolicy = Policy.NoOpAsync<HttpResponseMessage>();
    }

    public async Task<TResponse> GetAsync<TResponse>(HttpRequestModel requestModel)
    {
        using var client = _httpClientFactory.CreateClient();

        if (requestModel.QueryParams != null)
        {
            requestModel.Url = PrepareQueryParams(requestModel.Url, requestModel.QueryParams);
        }

        if (requestModel.PathVariables != null)
        {
            requestModel.Url = PreparePathVariables(requestModel.Url, requestModel.PathVariables);
        }

        if (requestModel.Headers != null)
        {
            foreach (var header in requestModel.Headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value.ToString());
            }
        }

        var policy = requestModel.RetryPolicy ? _defaultRetryPolicy : _noRetryPolicy;
        var response = await policy.ExecuteAsync(async () => 
            await client.GetAsync(requestModel.Url).ConfigureAwait(false));

        return await HandleResponse<TResponse>(response).ConfigureAwait(false);
    }

    public async Task<TResponse> PostAsync<TResponse>(HttpRequestModel requestModel)
    {
        using var client = _httpClientFactory.CreateClient();

        if (requestModel.PathVariables != null)
        {
            requestModel.Url = PreparePathVariables(requestModel.Url, requestModel.PathVariables);
        }

        if (requestModel.Headers != null)
        {
            foreach (var header in requestModel.Headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value.ToString());
            }
        }

        var content = PrepareContent(requestModel);

        var policy = requestModel.RetryPolicy ? _defaultRetryPolicy : _noRetryPolicy;
        var response = await policy.ExecuteAsync(async () =>
            await client.PostAsync(requestModel.Url, content).ConfigureAwait(false));

        return await HandleResponse<TResponse>(response).ConfigureAwait(false);
    }

    public async Task<TResponse> PutAsync<TResponse>(HttpRequestModel requestModel)
    {
        using var client = _httpClientFactory.CreateClient();

        if (requestModel.PathVariables != null)
        {
            requestModel.Url = PreparePathVariables(requestModel.Url, requestModel.PathVariables);
        }

        if (requestModel.Headers != null)
        {
            foreach (var header in requestModel.Headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value.ToString());
            }
        }

        var content = PrepareContent(requestModel);

        var policy = requestModel.RetryPolicy ? _defaultRetryPolicy : _noRetryPolicy;
        var response = await policy.ExecuteAsync(async () =>
            await client.PutAsync(requestModel.Url, content).ConfigureAwait(false));

        return await HandleResponse<TResponse>(response).ConfigureAwait(false);
    }

    public async Task<TResponse> DeleteAsync<TResponse>(HttpRequestModel requestModel)
    {
        using var client = _httpClientFactory.CreateClient();

        if (requestModel.PathVariables != null)
        {
            requestModel.Url = PreparePathVariables(requestModel.Url, requestModel.PathVariables);
        }

        if (requestModel.Headers != null)
        {
            foreach (var header in requestModel.Headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value.ToString());
            }
        }

        var policy = requestModel.RetryPolicy ? _defaultRetryPolicy : _noRetryPolicy;
        var response = await policy.ExecuteAsync(async () =>
            await client.DeleteAsync(requestModel.Url).ConfigureAwait(false));

        return await HandleResponse<TResponse>(response).ConfigureAwait(false);
    }

    protected virtual async Task<TResponse> HandleResponse<TResponse>(HttpResponseMessage response)
    {
        try
        {
            response.EnsureSuccessStatusCode();
            return await DeserializeContent<TResponse>(response).ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new ApplicationException($"Http request failed. Status: {response.StatusCode}, Response Content {content}");
        }
    }

    private async Task<TResponse> DeserializeContent<TResponse>(HttpResponseMessage response)
    {
        var contentType = response.Content.Headers.ContentType?.MediaType;

        if (contentType != null && contentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            return await DeserializeMultipartFormData<TResponse>(response);
        }

        return contentType switch
        {
            "application/json" => await DeserializeJson<TResponse>(response),
            "text/html" => await DeserializeJson<TResponse>(response)
        };
    }

    private async Task<TResponse> DeserializeMultipartFormData<TResponse>(HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();
        var boundry = GetBoundryFromContentType(response.Content.Headers.ContentType?.Parameters);

        throw new NotImplementedException("multipart/form-data parsing logic not implemented yet");
    }

    private string GetBoundryFromContentType(ICollection<NameValueHeaderValue>? parameters)
    {
        if (parameters == null)
        {
            throw new ArgumentNullException("content type has no parameters");
        }

        var boundryParam = parameters.FirstOrDefault(p => 
            p.Name.Equals("boundry", StringComparison.OrdinalIgnoreCase));

        if (boundryParam == null)
        {
            throw new InvalidOperationException("no boundry found in content type");
        }

        return boundryParam.Value.Trim('"');
    }

    private async Task<TResponse> DeserializeJson<TResponse>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = await response.Content.ReadFromJsonAsync<TResponse>().ConfigureAwait(false)
            ?? throw new InvalidOperationException("Failed to deserialize JSON response to the specified type");

        return result;
    }

    private static string PreparePathVariables(string url, Dictionary<string, object> pathVariables)
    {
        foreach (var pathVariable in pathVariables)
        {
            url = url.Replace($":{pathVariable.Key}", Uri.EscapeDataString(pathVariable.Value.ToString()));
        }

        return url;
    }

    private static string PrepareQueryParams(string url, Dictionary<string, object> queryParams)
    {
        if (queryParams != null && queryParams.Any())
        {
            var queryString = string.Join("&", queryParams.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value.ToString())}"));
            url = $"{url}?{queryString}";
        }

        return url;
    }

    private static HttpContent? PrepareContent(HttpRequestModel requestModel)
    {
        HttpContent? content = null;

        switch (requestModel.ContentType)
        {
            case var contentType when contentType == MediaTypeNames.Application.FormUrlEncoded:
                var formData = requestModel.RequestParameters
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? string.Empty);
                content = new FormUrlEncodedContent(formData);
                break;

            case var contentType when contentType == MediaTypeNames.Application.Json:
                var jsonContent = JsonSerializer.Serialize(requestModel.RequestParameters);
                content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.Json);
                break;
        }

        return content;
    }
}
