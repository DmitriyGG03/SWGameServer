using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Tests
{
	public class LobbyClient
	{
		const int _port = 7148;
		const string _hubName = "lobby";

		public Lobby? Lobby { get; set; }
		public Hero? Hero { get; set; }
		HubConnection _connection;

		public LobbyClient(WebApplicationFactory<Program> factory, HttpClient client)
		{
			_connection = new HubConnectionBuilder()
			.WithUrl(
				$"https://localhost:{_port}/hubs/{_hubName}",
				options =>
				{
					options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
					options.AccessTokenProvider = () => Task.FromResult(client.DefaultRequestHeaders.Authorization.Parameter);
				}).WithAutomaticReconnect().Build();

			_connection.On<string>(ClientHandlers.Lobby.Error, (errorMessage) =>
			{
				throw new Exception(errorMessage);
			});

			_connection.On<Lobby>(ClientHandlers.Lobby.ConnectToLobbyHandler, (lobby) =>
			{
				Lobby = lobby;
			});

			_connection.On<Lobby>(ClientHandlers.Lobby.ExitFromLobbyHandler, (lobby) =>
			{
				Lobby = null;
			});

			_connection.On<Lobby>(ClientHandlers.Lobby.ChangeLobbyDataHandler, (lobby) =>
			{
				Lobby = lobby;
			});
			_connection.On<string>(ClientHandlers.Lobby.DeleteLobbyHandler, (serverMessage) =>
			{
				Lobby = null;
			});

			_connection.On<Hero>(ClientHandlers.Lobby.CreatedSessionHandler, (hero) =>
			{
				Hero = hero;
			});

			_connection.StartAsync();
		}

		public void ConnectToLobby(Guid lobbyId)
		{
			_connection.InvokeAsync(ServerHandlers.Lobby.ConnectToLobby, lobbyId);
		}
		public void ExitFromLobby(Guid lobbyId)
		{
			_connection.InvokeAsync(ServerHandlers.Lobby.ExitFromLobby, lobbyId);
		}
		public void ChangeLobbyData()
		{

		}
		public void CreateSession()
		{
			if (Lobby is not null)
			{
				 _connection.InvokeAsync(ServerHandlers.Lobby.CreateSession, Lobby);
			}
		}
	}
}
