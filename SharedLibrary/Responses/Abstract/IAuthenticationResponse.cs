using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibrary.Responses.Abstract
{
	public interface IAuthenticationResponse
	{
		public string? Token { get; set; }
	}
}
