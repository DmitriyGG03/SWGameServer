using SharedLibrary.Responses.Abstract;
using System.Collections.Generic;
using System.Linq;

namespace SharedLibrary.Responses
{
	public class AuthenticationResponse : ResponseBase, IAuthenticationResponse
	{
		public string? Token { get; set; }

		public AuthenticationResponse()
		{ }

		public AuthenticationResponse(IEnumerable<string> info, string? token = null)
		{
			Info = info.ToArray();
			Token = token;
		}
	}
}


