using Azure.Core;

namespace Server.Domain
{
    public class AuthenticationResult
    {
        public bool Success { get => AccessToken is not null; }
        public string? AccessToken { get; set; }
        public string[] OperationInfo { get; set; }

		public AuthenticationResult(string[] info, string accessToken = null)
        {
			OperationInfo = info;
            AccessToken = accessToken;
        }
	}
}