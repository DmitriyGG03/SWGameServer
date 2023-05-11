using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedLibrary.Models
{
    [Table("Sessions"), Serializable]
    public class Session
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        
        public ICollection<Hero>? Heroes { get; set; }
        
        [ForeignKey(nameof(SessionMap))]
        public Guid SessionMapId { get; set; }
        public SessionMap? SessionMap { get; set; }

        public int TurnNumber { get; set; }
        public int ActiveHeroId { get; set; }
    }
}