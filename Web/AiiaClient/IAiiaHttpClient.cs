using System.Threading.Tasks;

namespace Aiia.Sample.Services;

public interface IAiiaHttpClient
{

    Task<T> HttpGet<T>(string url,
        AiiaAccessToken? accessTokens = null);

    Task<TResponse> HttpPost<TRequest, TResponse>(string url,
        TRequest body,
        AiiaAccessToken? accessTokens = null);
    
    Task<T> HttpGet<T>(string url,
        AiiaClientSecret clientSecret);

    Task<TResponse> HttpPost<TRequest, TResponse>(string url,
        TRequest body,
        AiiaClientSecret clientSecret);
}