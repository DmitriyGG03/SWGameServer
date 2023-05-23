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
        }
        public Planet(PointF position)
        {
            Id = Guid.NewGuid();
            X = position.X;
            Y = position.Y;
        }
    }
}
