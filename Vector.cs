namespace Calculator
{
    internal class Vector
    {
        public Fraction X, Y, Z;

        public Vector() { }

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
            {
                return n1.X == n2.X && n1.Y == n2.Y && n1.Z == n2.Z;
            }
            return false;
        }

        public static bool operator ==(object o, Vector n2)
        {
            if (o is Vector n1)
            {
                return n1.X == n2.X && n1.Y == n2.Y && n1.Z == n2.Z;
            }
            return false;
        }

        public static bool operator !=(Vector n1, Vector n2) => n1.X != n2.X || n1.Y != n2.Y || n1.Y == n2.Y;

        public static bool operator !=(Vector n1, object o)
        {
            if (o is Vector n2)
            {
                return n1.X != n2.X || n1.Y != n2.Y || n1.Z != n2.Z;
            }
            return true;
        }

        public static bool operator !=(object o, Vector n2)
        {
            if (o is Vector n1)
            {
                return n1.X != n2.X || n1.Y != n2.Y || n1.Z != n2.Z;
            }
            return true;
        }

        public override bool Equals(object o)
        {
            if (o is Vector n)
            {
                return X == n.X && Y == n.Y && Z == n.Z;
            }
            return false;
        }

        public static Vector Pow(Vector n, Fraction pow) => new Vector(
            Fraction.Pow(n.X, pow),
            Fraction.Pow(n.Y, pow),
            Fraction.Pow(n.Z, pow));

        public static Vector Pow(Fraction n, Vector pow) => new Vector(
            Fraction.Pow(n, pow.X),
            Fraction.Pow(n, pow.Y),
            Fraction.Pow(n, pow.Z));

        public static Vector Pow(Vector n, Vector pow) => new Vector(
            Fraction.Pow(n.X, pow.X),
            Fraction.Pow(n.Y, pow.Y),
            Fraction.Pow(n.Z, pow.Z));

        public static Vector Sin(Vector n) => new Vector(
            Fraction.Sin(n.X),
            Fraction.Sin(n.Y),
            Fraction.Sin(n.Z));

        public static Vector Asin(Vector n) => new Vector(
            Fraction.Asin(n.X),
            Fraction.Asin(n.Y),
            Fraction.Asin(n.Z));

        public static Vector Cos(Vector n) => new Vector(
            Fraction.Cos(n.X),
            Fraction.Cos(n.Y),
            Fraction.Cos(n.Z));

        public static Vector Acos(Vector n) => new Vector(
            Fraction.Acos(n.X),
            Fraction.Acos(n.Y),
            Fraction.Acos(n.Z));

        public static Vector Tan(Vector n) => new Vector(
            Fraction.Tan(n.X),
            Fraction.Tan(n.Y),
            Fraction.Tan(n.Z));

        public static Vector Atan(Vector n) => new Vector(
            Fraction.Atan(n.X),
            Fraction.Atan(n.Y),
            Fraction.Atan(n.Z));

        public static Vector Abs(Vector n) => new Vector(
            Fraction.Abs(n.X),
            Fraction.Abs(n.Y),
            Fraction.Abs(n.Z));

        public static Vector Floor(Vector n) => new Vector(
            Fraction.Floor(n.X),
            Fraction.Floor(n.Y),
            Fraction.Floor(n.Z));

        public static Vector Ceiling(Vector n) => new Vector(
            Fraction.Ceiling(n.X),
            Fraction.Ceiling(n.Y),
            Fraction.Ceiling(n.Z));

        public static Vector Round(Vector n) => new Vector(
            Fraction.Round(n.X),
            Fraction.Round(n.Y),
            Fraction.Round(n.Z));

        public static Vector Round(Vector n, int digits) => new Vector(
            Fraction.Round(n.X, digits),
            Fraction.Round(n.Y, digits),
            Fraction.Round(n.Z, digits));

        public static Vector Round(Vector n, Vector digits) => new Vector(
            Fraction.Round(n.X, (int)digits.X),
            Fraction.Round(n.Y, (int)digits.Y),
            Fraction.Round(n.Z, (int)digits.Z));

        public Vector Cross(Vector n) => new Vector(
            Y * n.Z - Z * n.Y,
            Z * n.X - X * n.Z,
            X * n.Y - Y * n.X);

        public Fraction Dot(Vector n) => X * n.X + Y * n.Y + Z * n.Z;

        public static Vector Min(Vector a, Vector b) => new Vector(
            Fraction.Min(a.X, b.X),
            Fraction.Min(a.Y, b.Y),
            Fraction.Min(a.Z, b.Z));

        public static Vector Min(Vector a, Fraction b) => new Vector(
            Fraction.Min(a.X, b),
            Fraction.Min(a.Y, b),
            Fraction.Min(a.Z, b));

        public static Vector Min(Fraction a, Vector b) => new Vector(
            Fraction.Min(a, b.X),
            Fraction.Min(a, b.Y),
            Fraction.Min(a, b.Z));

        public static Vector Max(Vector a, Vector b) => new Vector(
            Fraction.Max(a.X, b.X),
            Fraction.Max(a.Y, b.Y),
            Fraction.Max(a.Z, b.Z));

        public static Vector Max(Vector a, Fraction b) => new Vector(
            Fraction.Max(a.X, b),
            Fraction.Max(a.Y, b),
            Fraction.Max(a.Z, b));

        public static Vector Max(Fraction a, Vector b) => new Vector(
            Fraction.Max(a, b.X),
            Fraction.Max(a, b.Y),
            Fraction.Max(a, b.Z));
    }
}
