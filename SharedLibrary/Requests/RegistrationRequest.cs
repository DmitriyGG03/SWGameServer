using System;
using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Requests
{
	[Serializable]
	public class RegistrationRequest
	{
		[Required]
		public string Username { get; set; }
		[Required, EmailAddress]
		public string Email { get; set; }
		[Required, DataType(DataType.Password)]
		public string Password { get; set; }
	}
}