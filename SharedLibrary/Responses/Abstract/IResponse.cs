using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibrary.Responses.Abstract
{
	public interface IResponse
	{
		public string[]? Info { get; set; }
		public string? Token { get; set; }
	}
}
