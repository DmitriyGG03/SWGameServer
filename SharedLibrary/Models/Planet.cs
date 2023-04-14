using System;

namespace SharedLibrary.Models
{
    public class Planet
    {
        public Point Position { get; set; }
        /* ctor for deserialization */
        public Planet()
        { }
        public Planet(Point position)
        {
            Position = position;
        }
    }
}
