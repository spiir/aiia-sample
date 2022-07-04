namespace Aiia.Sample.AiiaClient;

public struct AiiaAccessToken
{
    public AiiaAccessToken(string tokenScheme, string token)
    {
        Token = token;
        TokenScheme = tokenScheme;
    }

    public string Token { get; set; }

    public string TokenScheme { get; set; }
}