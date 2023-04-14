namespace SharedLibrary.Models
{
    public class Point
    {
        public float X { get; set; }
        public float Y { get; set; }
        /* ctor for deserialization */
        public Point()
        { }
        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}