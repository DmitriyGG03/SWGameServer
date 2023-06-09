using System;
using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Requests
{
    [Serializable]
    public class CreateLobbyRequest
    {
        [Required]
        public string LobbyName { get; set; }
        [Range(0, 100)]
        public byte MaxUsersCount { get; set; }
        [Required] 
        public string FutureHeroName { get; set; }

        public CreateLobbyRequest(string lobbyName, byte maxUsersCount = 2)
        {
            LobbyName = lobbyName;
            MaxUsersCount = maxUsersCount; 
            FutureHeroName = String.Empty;
        }
    }
}