using System;
using SharedLibrary.Responses.Abstract;
using System.Collections.Generic;
using System.Linq;

namespace SharedLibrary.Responses
{
	[Serializable]
	public class AuthenticationResponse : ResponseBase, IAuthenticationResponse
	{
		public Guid UserId { get; set; }
		public string? Token { get; set; }

		public AuthenticationResponse()
		{ }

		public AuthenticationResponse(IEnumerable<string> info, string? token = null)
		{
			Info = info.ToArray();
			Token = token;
		}
		public AuthenticationResponse(IEnumerable<string> info, Guid userId, string? token = null)
		{
			Info = info.ToArray();
			Token = token;
			UserId = userId;
		}
	}
}


