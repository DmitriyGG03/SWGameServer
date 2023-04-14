using System.Drawing;

namespace drawing_test.Models
{
    public class Planet
    {
        public PointF Position { get; set; }
        public Planet(PointF position)
        {
            Position = position;
        }
    }
}
