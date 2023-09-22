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

        public static Vector operator +(Vector v1, Fraction v2) => new Vector(v1.X + v2, v1.Y + v2, v1.Z + v2);

        public static Vector operator +(Fraction v1, Vector v2) => new Vector(v1 + v2.X, v1 + v2.Y, v1 + v2.Z);

        public static Vector operator +(Vector v1, Vector v2) => new Vector(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);

        public static Vector operator -(Vector v1) => new Vector(-v1.X, -v1.Y, -v1.Z);

        public static Vector operator -(Vector v1, Fraction v2) => new Vector(v1.X - v2, v1.Y - v2, v1.Z - v2);

        public static Vector operator -(Fraction v1, Vector v2) => new Vector(v1 - v2.X, v1 - v2.Y, v1 - v2.Z);

        public static Vector operator -(Vector v1, Vector v2) => new Vector(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);

        public static Vector operator *(Vector v1, Fraction v2) => new Vector(v1.X * v2, v1.Y * v2, v1.Z * v2);

        public static Vector operator *(Fraction v1, Vector v2) => new Vector(v1 * v2.X, v1 * v2.Y, v1 * v2.Z);

        public static Vector operator *(Vector v1, Vector v2) => new Vector(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);

        public static Vector operator /(Vector v1, Fraction v2) => new Vector(v1.X / v2, v1.Y / v2, v1.Z / v2);

        public static Vector operator /(Fraction v1, Vector v2) => new Vector(v1 / v2.X, v1 / v2.Y, v1 / v2.Z);

        public static Vector operator /(Vector v1, Vector v2) => new Vector(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);

        public static Vector operator %(Vector v1, Fraction v2) => new Vector(v1.X % v2, v1.Y % v2, v1.Z % v2);

        public static Vector operator %(Fraction v1, Vector v2) => new Vector(v1 % v2.X, v1 % v2.Y, v1 % v2.Z);

        public static Vector operator %(Vector v1, Vector v2) => new Vector(v1.X % v2.X, v1.Y % v2.Y, v1.Z % (Fraction)v2.Z);

        public static bool operator ==(Vector v1, Vector v2) => v1.X == v2.X && v1.Y == v2.Y && v1.Y == v2.Y;

        public static bool operator ==(Vector v1, object o)
        {
            if (o is Vector v2) return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
            return false;
        }

        public static bool operator ==(object o, Vector v2)
        {
            if (o is Vector v1) return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
            return false;
        }

        public static bool operator !=(Vector v1, Vector v2) => v1.X != v2.X || v1.Y != v2.Y || v1.Y == v2.Y;

        public static bool operator !=(Vector v1, object o)
        {
            if (o is Vector v2) return v1.X != v2.X || v1.Y != v2.Y || v1.Z != v2.Z;
            return true;
        }

        public static bool operator !=(object o, Vector v2)
        {
            if (o is Vector v1) return v1.X != v2.X || v1.Y != v2.Y || v1.Z != v2.Z;
            return true;
        }

        public override bool Equals(object o)
        {
            if (o is Vector v) return X == v.X && Y == v.Y && Z == v.Z;
            return false;
        }

        public Vector Cross(Vector n) => new Vector(
            Y * n.Z - Z * n.Y,
            Z * n.X - X * n.Z,
            X * n.Y - Y * n.X);

        public Fraction Dot(Vector n) => X * n.X + Y * n.Y + Z * n.Z;
    }
}
