using Azure.Core;
using SharedLibrary.Models;

namespace Server.Domain
{
    public class AuthenticationResult
    {
        public bool Success { get => AccessToken is not null; }
        public string? AccessToken { get; set; }
        public string[] OperationInfo { get; set; }
        public ApplicationUser? User { get; set; }

		public AuthenticationResult(string[] info, string accessToken = null)
        {
			OperationInfo = info;
            AccessToken = accessToken;
            User = null;
        }
		public AuthenticationResult(string[] info, ApplicationUser user, string accessToken = null)
		{
			OperationInfo = info;
			AccessToken = accessToken;
			User = user;
		}
	}
}