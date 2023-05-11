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
        /* ctor for deserialization */
        public Planet()
        { }
        public Planet(Point position)
        {
            Id = Guid.NewGuid();
            Position = position;
        }
    }
}
