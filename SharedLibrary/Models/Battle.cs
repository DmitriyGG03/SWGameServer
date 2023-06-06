using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SharedLibrary.Models.Enums;

namespace SharedLibrary.Models
{
    public class Battle
    {
        [Key]
        public Guid Id { get; set; }
        
        [ForeignKey(nameof(AttackerHero))]
        public Guid AttackerHeroId { get; set; }
        public Hero? AttackerHero { get; set; }

        [ForeignKey(nameof(DefendingHero))] 
        public Guid DefendingHeroId { get; set; }
        public Hero? DefendingHero { get; set; }

        [ForeignKey(nameof(AttackedPlanet))]
        public Guid AttackedPlanetId { get; set; }
        public Planet? AttackedPlanet { get; set; }
        
        [ForeignKey(nameof(AttackedFrom))]
        public Guid AttackedFromId { get; set; }
        public Planet? AttackedFrom { get; set; }

        public BattleStatus Status { get; set; } = BattleStatus.InProcess;
        public bool Display { get; set; } = true;
        public int BattleTurnNumber { get; set; } = 0;
    }
}