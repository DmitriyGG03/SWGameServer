using System.Drawing;

namespace drawing_test.Math;

public static class Geometry
{
        public static PointF GenerateRandomPoint(int width, int height)
        {
            return new PointF(Random.Shared.Next(width), Random.Shared.Next(height));
        }
        public static float CalculateDistance(PointF p1, PointF p2)
        {
            float dx = p1.X - p2.X;
            float dy = p1.Y - p2.Y;
            return (float)System.Math.Sqrt(dx * dx + dy * dy);
        }
        public static bool Intersects(PointF a1, PointF a2, PointF b1, PointF b2)
        {
            /* some smart algorithm */
            float d1 = CrossProduct(new PointF(b1.X - a1.X, b1.Y - a1.Y), new PointF(a2.X - a1.X, a2.Y - a1.Y));
            float d2 = CrossProduct(new PointF(b2.X - a1.X, b2.Y - a1.Y), new PointF(a2.X - a1.X, a2.Y - a1.Y));
            float d3 = CrossProduct(new PointF(a1.X - b1.X, a1.Y - b1.Y), new PointF(b2.X - b1.X, b2.Y - b1.Y));
            float d4 = CrossProduct(new PointF(a2.X - b1.X, a2.Y - b1.Y), new PointF(b2.X - b1.X, b2.Y - b1.Y));
            
            if (d1 * d2 < 0 && d3 * d4 < 0)
            {
                return true;
            }
            if (d1 == 0 && OnSegment(a1, a2, b1))
            {
                return true;
            }
            if (d2 == 0 && OnSegment(a1, a2, b2))
            {
                return true;
            }
            if (d3 == 0 && OnSegment(b1, b2, a1))
            {
                return true;
            }
            if (d4 == 0 && OnSegment(b1, b2, a2))
            {
                return true;
            }
            
            return false;
        }
        public static float CrossProduct(PointF a, PointF b)
        {
            return a.X * b.Y - b.X * a.Y;
        }
        public static bool OnSegment(PointF a, PointF b, PointF c)
        {
            if (c.X >= System.Math.Min(a.X, b.X) && c.X <= System.Math.Max(a.X, b.X) &&
                c.Y >= System.Math.Min(a.Y, b.Y) && c.Y <= System.Math.Max(a.Y, b.Y))
            {
                return true;
            }
            return false;
        }
        public static float CalculateDistanceToSegment(PointF a, PointF b, PointF p)
        {
            float l2 = CalculateDistance(a, b) * CalculateDistance(a, b);
            if (l2 == 0)
            {
                return CalculateDistance(p, a);
            }

            float t = ((p.X - a.X) * (b.X - a.X) + (p.Y - a.Y) * (b.Y - a.Y)) / l2;
            if (t < 0)
            {
                return CalculateDistance(p, a);
            }
            else if (t > 1)
            {
                return CalculateDistance(p, b);
            }

            PointF projection = new PointF(a.X + t * (b.X - a.X), a.Y + t * (b.Y - a.Y));
            return CalculateDistance(p, projection);
        }
}