using System;

namespace SharedLibrary.Models
{
    public class Point
    {
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