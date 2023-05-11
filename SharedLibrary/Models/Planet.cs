using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedLibrary.Models
{
    [Table("Planets"), Serializable]
    public class Planet
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public Guid Id { get; set; }
        public Point Position { get; set; }
        [Range(0, Int32.MaxValue)]
        public int Status { get; set; }
        [Range(0, byte.MaxValue)]
        public byte DaysNumber { get; set; }
        /* ctor for deserialization */
        public Planet()
        {
            Status = (int)PlanetStatus.Known;
        }
        public Planet(Point position)
        {
            Id = Guid.NewGuid();
            Position = position;
            Status = (int) PlanetStatus.Researched;
        }
    }
}
