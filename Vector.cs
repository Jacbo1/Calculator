using System;

namespace Calculator
{
    internal class Vector
    {
        public double X, Y, Z;

        public Vector() { }

        public Vector(double n)
        {
            X = n;
            Y = n;
            Z = n;
        }

        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static implicit operator Vector(double n) => new Vector(n);

        public static Vector operator - (Vector n1) => new Vector(-n1.X, -n1.Y, -n1.Z);

        public static Vector operator + (Vector n1, double n2) => new Vector(n1.X + n2, n1.Y + n2, n1.Z + n2);

        public static Vector operator + (double n1, Vector n2) => new Vector(n1 + n2.X, n1 + n2.Y, n1 + n2.Z);

        public static Vector operator + (Vector n1, Vector n2) => new Vector(n1.X + n2.X, n2.Y + n2.Y, n1.Z + n2.Z);

        public static Vector operator - (Vector n1, double n2) => new Vector(n1.X - n2, n1.Y - n2, n1.Z - n2);

        public static Vector operator - (double n1, Vector n2) => new Vector(n1 - n2.X, n1 - n2.Y, n1 - n2.Z);

        public static Vector operator - (Vector n1, Vector n2) => new Vector(n1.X - n2.X, n1.Y - n2.Y, n1.Z - n2.Z);

        public static Vector operator * (Vector n1, double n2) => new Vector(n1.X * n2, n1.Y * n2, n1.Z * n2);

        public static Vector operator * (double n1, Vector n2) => new Vector(n1 * n2.X, n1 * n2.Y, n1 * n2.Z);

        public static Vector operator * (Vector n1, Vector n2) => new Vector(n1.X * n2.X, n1.Y * n2.Y, n1.Z * n2.Z);

        public static Vector operator / (Vector n1, double n2) => new Vector(n1.X / n2, n1.Y / n2, n1.Z / n2);

        public static Vector operator / (double n1, Vector n2) => new Vector(n1 / n2.X, n1 / n2.Y, n1 / n2.Z);

        public static Vector operator / (Vector n1, Vector n2) => new Vector(n1.X / n2.X, n1.Y / n2.Y, n1.Z / n2.Z);

        public static Vector operator % (Vector n1, double n2) => new Vector((double)n1.X % (double)n2, (double)n1.Y % (double)n2, (double)n1.Z % (double)n2);

        public static Vector operator % (double n1, Vector n2) => new Vector((double)n1 % (double)n2.X, (double)n1 % (double)n2.Y, (double)n1 % (double)n2.Z);

        public static Vector operator % (Vector n1, Vector n2) => new Vector((double)n1.X % (double)n2.X, (double)n1.Y % (double)n2.Y, (double)n1.Z % (double)n2.Z);

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

        public static Vector Pow(Vector n, double pow) => new Vector(
            Math.Pow(n.X, pow),
            Math.Pow(n.Y, pow),
            Math.Pow(n.Z, pow));

        public static Vector Pow(double n, Vector pow) => new Vector(
            Math.Pow(n, pow.X),
            Math.Pow(n, pow.Y),
            Math.Pow(n, pow.Z));

        public static Vector Pow(Vector n, Vector pow) => new Vector(
            Math.Pow(n.X, pow.X),
            Math.Pow(n.Y, pow.Y),
            Math.Pow(n.Z, pow.Z));

        public static Vector Sqrt(Vector n) => new Vector(
            Math.Sqrt(n.X),
            Math.Sqrt(n.Y),
            Math.Sqrt(n.Z));

        public static Vector Sin(Vector n) => new Vector(
            Math.Sin(n.X),
            Math.Sin(n.Y),
            Math.Sin(n.Z));

        public static Vector Asin(Vector n) => new Vector(
            Math.Asin(n.X),
            Math.Asin(n.Y),
            Math.Asin(n.Z));

        public static Vector Cos(Vector n) => new Vector(
            Math.Cos(n.X),
            Math.Cos(n.Y),
            Math.Cos(n.Z));

        public static Vector Acos(Vector n) => new Vector(
            Math.Acos(n.X),
            Math.Acos(n.Y),
            Math.Acos(n.Z));

        public static Vector Tan(Vector n) => new Vector(
            Math.Tan(n.X),
            Math.Tan(n.Y),
            Math.Tan(n.Z));

        public static Vector Atan(Vector n) => new Vector(
            Math.Atan(n.X),
            Math.Atan(n.Y),
            Math.Atan(n.Z));

        public static Vector Abs(Vector n) => new Vector(
            Math.Abs(n.X),
            Math.Abs(n.Y),
            Math.Abs(n.Z));

        public static Vector Floor(Vector n) => new Vector(
            Math.Floor(n.X),
            Math.Floor(n.Y),
            Math.Floor(n.Z));

        public static Vector Ceiling(Vector n) => new Vector(
            Math.Ceiling(n.X),
            Math.Ceiling(n.Y),
            Math.Ceiling(n.Z));

        public static Vector Round(Vector n) => new Vector(
            Math.Round(n.X),
            Math.Round(n.Y),
            Math.Round(n.Z));

        public Vector Cross(Vector n) => new Vector(
            Y * n.Z - Z * n.Y,
            Z * n.X - X * n.Z,
            X * n.Y - Y * n.X);

        public double Dot(Vector n) => X * n.X + Y * n.Y + Z * n.Z;
    }
}
