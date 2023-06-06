using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedLibrary.Models
{
    [Table("Lobbies"), Serializable]
    public class Lobby
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public Guid Id { get; set; }
        [Required, Column(TypeName = "nvarchar(256)")]
        public string LobbyName { get; set; }
        [Range(0, byte.MaxValue)]
        public byte MaxHeroNumbers { get; set; }

        public bool Visible { get; set; } = true;
        
        public ICollection<LobbyInfo>? LobbyInfos { get; set; }
    }
}