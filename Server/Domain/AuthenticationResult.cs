namespace Server.Domain
{
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string? AccessToken { get; set; }
        public string[]? Errors { get; set; }

        public AuthenticationResult(string accessToken)
        {
            Success = true;
            AccessToken = accessToken;
            Errors = null;
        }
        public AuthenticationResult(string[] errors)
        {
            Success = false;
            Errors = errors;
            AccessToken = null;
        }
    }
}