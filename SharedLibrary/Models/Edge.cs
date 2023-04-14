using System.Drawing;
using drawing_test.Math;

namespace drawing_test.Models
{
    public class Edge
    {
        public Planet From { get; }
        public Planet To { get; }
        public Edge(Planet from, Planet to)
        {
            From = from;
            To = to;
        }
    }
}
