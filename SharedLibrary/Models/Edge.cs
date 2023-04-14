using System;

namespace SharedLibrary.Models
{
    public class Edge
    {
        public Planet From { get; set; }
        public Planet To { get; set; }
        /* ctor for deserialization */
        public Edge()
        { }

        public Edge(Planet from, Planet to)
        {
            From = from;
            To = to;
        }
    }
}
