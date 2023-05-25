using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace SharedLibrary.Models
{
    [Table("LobbyInfos"), Serializable]
    public class LobbyInfo
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public Guid Id { get; set; }
        public int Argb { get; set; }
        [NotMapped]
        public Color Color { get => Color.FromArgb(Argb); }
        public bool Ready { get; set; }
        public bool LobbyLeader { get; set; }
        
        [ForeignKey(nameof(Lobby))]
        public Guid LobbyId { get; set; }
        public Lobby? Lobby { get; set; }
        
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}