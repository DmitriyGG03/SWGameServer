using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Requests
{
    public class CreateLobbyRequest
    {
        [Required]
        public string LobbyName { get; set; }
        [Range(0, 100)]
        public byte MaxUsersCount { get; set; }

        public CreateLobbyRequest(string lobbyName, byte maxUsersCount = 2)
        {
            LobbyName = lobbyName;
            MaxUsersCount = maxUsersCount; 
        }
    }
}