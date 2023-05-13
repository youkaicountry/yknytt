using System;

namespace YKnyttLib
{
    public struct KnyttPoint
    {
        public readonly int x, y;
        public int Area 
        { 
            get { return x * y; }
        }

        public static KnyttPoint Zero { get { return new KnyttPoint(0, 0); } }

        public KnyttPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public KnyttPoint min(KnyttPoint point)
        {
            return new KnyttPoint(Math.Min(point.x, x), Math.Min(point.y, y));
        }

        public KnyttPoint max(KnyttPoint point)
        {
            return new KnyttPoint(Math.Max(point.x, x), Math.Max(point.y, y));
        }

        public int manhattanDistance(KnyttPoint point)
        {
            return Math.Abs(point.x - x) + Math.Abs(point.y - y);
        }

        public bool isZero() { return this.x == 0 && this.y == 0; }

        public static KnyttPoint operator +(KnyttPoint a, KnyttPoint b)
        => new KnyttPoint(a.x + b.x, a.y + b.y);

        public static KnyttPoint operator -(KnyttPoint a, KnyttPoint b)
        => new KnyttPoint(a.x - b.x, a.y - b.y);

        public static KnyttPoint operator *(KnyttPoint a, KnyttPoint b)
        => new KnyttPoint(a.x * b.x, a.y * b.y);

        public static KnyttPoint operator /(KnyttPoint a, KnyttPoint b)
        {
            if (b.x == 0 || b.y == 0) { throw new DivideByZeroException(); }
            return new KnyttPoint(a.x / b.x, a.y / b.y);
        }

        public override bool Equals(object obj)
        {
            return ((KnyttPoint)obj).x == this.x && ((KnyttPoint)obj).y == this.y;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", this.x, this.y);
        }
    }
}
