namespace Calculator
{
    public struct Vector
    {
        public Fraction X, Y, Z;

        public Vector(Fraction n)
        {
            X = n;
            Y = n;
            Z = n;
        }

        public Vector(Fraction x, Fraction y)
        {
            X = x;
            Y = y;
            Z = 0;
        }

        public Vector(Fraction x, Fraction y, Fraction z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static implicit operator Vector(Fraction n) => new Vector(n);

        public static Vector operator +(Vector n1, Fraction n2) => new Vector(n1.X + n2, n1.Y + n2, n1.Z + n2);

        public static Vector operator +(Fraction n1, Vector n2) => new Vector(n1 + n2.X, n1 + n2.Y, n1 + n2.Z);

        public static Vector operator +(Vector n1, Vector n2) => new Vector(n1.X + n2.X, n1.Y + n2.Y, n1.Z + n2.Z);

        public static Vector operator -(Vector n1) => new Vector(-n1.X, -n1.Y, -n1.Z);

        public static Vector operator -(Vector n1, Fraction n2) => new Vector(n1.X - n2, n1.Y - n2, n1.Z - n2);

        public static Vector operator -(Fraction n1, Vector n2) => new Vector(n1 - n2.X, n1 - n2.Y, n1 - n2.Z);

        public static Vector operator -(Vector n1, Vector n2) => new Vector(n1.X - n2.X, n1.Y - n2.Y, n1.Z - n2.Z);

        public static Vector operator *(Vector n1, Fraction n2) => new Vector(n1.X * n2, n1.Y * n2, n1.Z * n2);

        public static Vector operator *(Fraction n1, Vector n2) => new Vector(n1 * n2.X, n1 * n2.Y, n1 * n2.Z);

        public static Vector operator *(Vector n1, Vector n2) => new Vector(n1.X * n2.X, n1.Y * n2.Y, n1.Z * n2.Z);

        public static Vector operator /(Vector n1, Fraction n2) => new Vector(n1.X / n2, n1.Y / n2, n1.Z / n2);

        public static Vector operator /(Fraction n1, Vector n2) => new Vector(n1 / n2.X, n1 / n2.Y, n1 / n2.Z);

        public static Vector operator /(Vector n1, Vector n2) => new Vector(n1.X / n2.X, n1.Y / n2.Y, n1.Z / n2.Z);

        public static Vector operator %(Vector n1, Fraction n2) => new Vector(n1.X % n2, n1.Y % n2, n1.Z % n2);

        public static Vector operator %(Fraction n1, Vector n2) => new Vector(n1 % n2.X, n1 % n2.Y, n1 % n2.Z);

        public static Vector operator %(Vector n1, Vector n2) => new Vector(n1.X % n2.X, n1.Y % n2.Y, n1.Z % (Fraction)n2.Z);

        public static bool operator ==(Vector n1, Vector n2) => n1.X == n2.X && n1.Y == n2.Y && n1.Y == n2.Y;

        public static bool operator ==(Vector n1, object o)
        {
            if (o is Vector n2)
                return n1.X == n2.X && n1.Y == n2.Y && n1.Z == n2.Z;
            return false;
        }

        public static bool operator ==(object o, Vector n2)
        {
            if (o is Vector n1)
                return n1.X == n2.X && n1.Y == n2.Y && n1.Z == n2.Z;
            return false;
        }

        public static bool operator !=(Vector n1, Vector n2) => n1.X != n2.X || n1.Y != n2.Y || n1.Y == n2.Y;

        public static bool operator !=(Vector n1, object o)
        {
            if (o is Vector n2)
                return n1.X != n2.X || n1.Y != n2.Y || n1.Z != n2.Z;
            return true;
        }

        public static bool operator !=(object o, Vector n2)
        {
            if (o is Vector n1)
                return n1.X != n2.X || n1.Y != n2.Y || n1.Z != n2.Z;
            return true;
        }

        public override bool Equals(object o)
        {
            if (o is Vector n)
                return X == n.X && Y == n.Y && Z == n.Z;
            return false;
        }

        public Vector Cross(Vector n) => new Vector(
            Y * n.Z - Z * n.Y,
            Z * n.X - X * n.Z,
            X * n.Y - Y * n.X);

        public Fraction Dot(Vector n) => X * n.X + Y * n.Y + Z * n.Z;
    }
}
