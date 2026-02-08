using System;

namespace YKnyttLib
{
    public struct KnyttPoint
    {
        public readonly int X, Y;
        public int Area
        {
            get { return X * Y; }
        }

        public static KnyttPoint Zero => new KnyttPoint(0, 0); 

        public KnyttPoint(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public KnyttPoint min(KnyttPoint point)
        {
            return new KnyttPoint(Math.Min(point.X, X), Math.Min(point.Y, Y));
        }

        public KnyttPoint max(KnyttPoint point)
        {
            return new KnyttPoint(Math.Max(point.X, X), Math.Max(point.Y, Y));
        }

        public int manhattanDistance(KnyttPoint point)
        {
            return Math.Abs(point.X - X) + Math.Abs(point.Y - Y);
        }

        public bool isZero() { return this.X == 0 && this.Y == 0; }

        public static KnyttPoint operator +(KnyttPoint a, KnyttPoint b)
        => new KnyttPoint(a.X + b.X, a.Y + b.Y);

        public static KnyttPoint operator -(KnyttPoint a, KnyttPoint b)
        => new KnyttPoint(a.X - b.X, a.Y - b.Y);

        public static KnyttPoint operator *(KnyttPoint a, KnyttPoint b)
        => new KnyttPoint(a.X * b.X, a.Y * b.Y);

        public static KnyttPoint operator /(KnyttPoint a, KnyttPoint b)
        {
            if (b.X == 0 || b.Y == 0) { throw new DivideByZeroException(); }
            return new KnyttPoint(a.X / b.X, a.Y / b.Y);
        }

        public static KnyttPoint operator %(KnyttPoint a, KnyttPoint b)
        => new KnyttPoint(positiveMod(a.X, b.X), positiveMod(a.Y, b.Y));

        public static int positiveMod(int x, int m) { return (x % m + m) % m; }
        
        public override bool Equals(object obj)
        {
            return ((KnyttPoint)obj).X == this.X && ((KnyttPoint)obj).Y == this.Y;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", this.X, this.Y);
        }
    }
}
