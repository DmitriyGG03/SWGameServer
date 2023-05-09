using SharedLibrary.Models;
using SharedLibrary.Responses.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedLibrary.Responses
{
	public class GetAllLobbiesResponse : ResponseBase
	{
		public IList<Lobby> Lobbies { get; set; }

		public GetAllLobbiesResponse(IEnumerable<string> info, IList<Lobby> lobbies = null!)
		{
			Info = info.ToArray();
			Lobbies = lobbies;
		}

		public GetAllLobbiesResponse() { }
	}
}
