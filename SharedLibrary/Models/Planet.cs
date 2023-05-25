using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace SharedLibrary.Models
{
    [Table("Planets"), Serializable]
    public class Planet
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public Guid Id { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        [Range(0, byte.MaxValue)]
        public byte DaysNumber { get; set; }
        [Range(0, int.MaxValue)]
        public int Size { get; set; }

        public string PlanetName { get; set; }
        public Guid? OwnerId { get; set; }
        
        [NotMapped, JsonIgnore, IgnoreDataMember]
        public PointF Position
        {
            get => new PointF(X, Y);
        }
        [NotMapped] 
        public bool IsEnemy { get; set; }
        [NotMapped] 
        public int Status { get; set; }

        public Planet()
        {
            Id = Guid.Empty;
            X = Y = 0;
            Size = new Random().Next(1, 1000);
            PlanetName = String.Empty;
        }
        public Planet(PointF position)
        {
            Id = Guid.NewGuid();
            X = position.X;
            Y = position.Y;
            Size = new Random().Next(1, 1000);
            PlanetName = String.Empty;
        }
        public Planet(PointF position, int size, string planetName)
        {
            Id = Guid.NewGuid();
            X = position.X;
            Y = position.Y;
            Size = size;
            PlanetName = planetName;
        }
    }
}
