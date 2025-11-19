namespace PayTR.PosSelection.Shared.SeedWork.HttpClient;

public interface IHttpClientService
{
    Task<TResponse> GetAsync<TResponse>(HttpRequestModel httpRequestModel);
    Task<TResponse> PostAsync<TResponse>(HttpRequestModel httpRequestModel);
    Task<TResponse> PutAsync<TResponse>(HttpRequestModel httpRequestModel);
    Task<TResponse> DeleteAsync<TResponse>(HttpRequestModel httpRequestModel);
}
