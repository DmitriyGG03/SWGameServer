using System.Collections.Generic;
using System.Linq;

namespace SharedLibrary.Responses
{
	public class AuthenticationResponse
	{
		public string[] Info { get; set; }
		public string? Token { get; set; }

		public AuthenticationResponse(IEnumerable<string> info, string? token = null)
		{
			Info = info.ToArray();
			Token = token;
		}
		public AuthenticationResponse() { } //For correct JSON deserialization
	}
}


