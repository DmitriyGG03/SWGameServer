using System;

namespace SharedLibrary.Models
{
    public class Planet
    {
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
