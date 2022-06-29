namespace Aiia.Sample.Services
{
    public struct AiiaAccessTokens
    {
        public AiiaAccessTokens(string tokenType, string accessToken)
        {
            AccessToken = accessToken;
            TokenType = tokenType;
        }

        // TODO: Rename to Token
        public string AccessToken { get; set; }
        
        // TODO: rename to TokenScheme
        public string TokenType { get; set; }
        
    }
}