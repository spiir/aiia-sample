using System.Threading.Tasks;

namespace Aiia.Sample.Services;

public interface IAiiaHttpClient
{

    Task<T> HttpGet<T>(string url,
        AiiaAccessTokens? accessTokens = null);

    Task<TResponse> HttpPost<TRequest, TResponse>(string url,
        TRequest body,
        AiiaAccessTokens? accessTokens = null);
    
    Task<T> HttpGet<T>(string url,
        AiiaClientSecrets clientSecrets);

    Task<TResponse> HttpPost<TRequest, TResponse>(string url,
        TRequest body,
        AiiaClientSecrets clientSecrets);
}