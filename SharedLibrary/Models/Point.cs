using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedLibrary.Models
{
    [Table("Points"), Serializable]
    public class Point
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public Guid Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        /* ctor for deserialization */
        public Point()
        { }
        public Point(float x, float y)
        {
            Id = Guid.NewGuid();
            X = x;
            Y = y;
        }
    }
}