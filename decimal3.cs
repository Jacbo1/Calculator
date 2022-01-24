using KGySoft.CoreLibraries;
using System;

namespace Calculator
{
    internal class decimal3
    {
        public decimal X, Y, Z;

        public decimal3() { }

        public decimal3(decimal n)
        {
            X = n;
            Y = n;
            Z = n;
        }

        public decimal3(decimal x, decimal y)
        {
            X = x;
            Y = y;
        }

        public decimal3(decimal x, decimal y, decimal z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static implicit operator decimal3(decimal n) => new decimal3(n);

        public static decimal3 operator - (decimal3 n1) => new decimal3(-n1.X, -n1.Y, -n1.Z);

        public static decimal3 operator + (decimal3 n1, decimal n2) => new decimal3(n1.X + n2, n1.Y + n2, n1.Z + n2);

        public static decimal3 operator + (decimal n1, decimal3 n2) => new decimal3(n1 + n2.X, n1 + n2.Y, n1 + n2.Z);

        public static decimal3 operator + (decimal3 n1, decimal3 n2) => new decimal3(n1.X + n2.X, n2.Y + n2.Y, n1.Z + n2.Z);

        public static decimal3 operator - (decimal3 n1, decimal n2) => new decimal3(n1.X - n2, n1.Y - n2, n1.Z - n2);

        public static decimal3 operator - (decimal n1, decimal3 n2) => new decimal3(n1 - n2.X, n1 - n2.Y, n1 - n2.Z);

        public static decimal3 operator - (decimal3 n1, decimal3 n2) => new decimal3(n1.X - n2.X, n1.Y - n2.Y, n1.Z - n2.Z);

        public static decimal3 operator * (decimal3 n1, decimal n2) => new decimal3(n1.X * n2, n1.Y * n2, n1.Z * n2);

        public static decimal3 operator * (decimal n1, decimal3 n2) => new decimal3(n1 * n2.X, n1 * n2.Y, n1 * n2.Z);

        public static decimal3 operator * (decimal3 n1, decimal3 n2) => new decimal3(n1.X * n2.X, n1.Y * n2.Y, n1.Z * n2.Z);

        public static decimal3 operator / (decimal3 n1, decimal n2) => new decimal3(n1.X / n2, n1.Y / n2, n1.Z / n2);

        public static decimal3 operator / (decimal n1, decimal3 n2) => new decimal3(n1 / n2.X, n1 / n2.Y, n1 / n2.Z);

        public static decimal3 operator / (decimal3 n1, decimal3 n2) => new decimal3(n1.X / n2.X, n1.Y / n2.Y, n1.Z / n2.Z);

        public static decimal3 operator % (decimal3 n1, decimal n2) => new decimal3(n1.X % n2, n1.Y % n2, n1.Z % n2);

        public static decimal3 operator % (decimal n1, decimal3 n2) => new decimal3(n1 % n2.X, n1 % n2.Y, n1 % n2.Z);

        public static decimal3 operator % (decimal3 n1, decimal3 n2) => new decimal3(n1.X % n2.X, n1.Y % n2.Y, n1.Z % n2.Z);

        public static bool operator ==(decimal3 n1, decimal3 n2) => n1.X == n2.X && n1.Y == n2.Y && n1.Y == n2.Y;

        public static bool operator ==(decimal3 n1, object o)
        {
            if (o is decimal3 n2)
            {
                return n1.X == n2.X && n1.Y == n2.Y && n1.Z == n2.Z;
            }
            return false;
        }

        public static bool operator ==(object o, decimal3 n2)
        {
            if (o is decimal3 n1)
            {
                return n1.X == n2.X && n1.Y == n2.Y && n1.Z == n2.Z;
            }
            return false;
        }

        public static bool operator !=(decimal3 n1, decimal3 n2) => n1.X != n2.X || n1.Y != n2.Y || n1.Y == n2.Y;

        public static bool operator !=(decimal3 n1, object o)
        {
            if (o is decimal3 n2)
            {
                return n1.X != n2.X || n1.Y != n2.Y || n1.Z != n2.Z;
            }
            return true;
        }

        public static bool operator !=(object o, decimal3 n2)
        {
            if (o is decimal3 n1)
            {
                return n1.X != n2.X || n1.Y != n2.Y || n1.Z != n2.Z;
            }
            return true;
        }

        public override bool Equals(object o)
        {
            if (o is decimal3 n)
            {
                return X == n.X && Y == n.Y && Z == n.Z;
            }
            return false;
        }

        public static decimal3 Pow(decimal3 n, decimal pow) => new decimal3(
            DecimalExtensions.Pow(n.X, pow),
            DecimalExtensions.Pow(n.Y, pow),
            DecimalExtensions.Pow(n.Z, pow));

        public static decimal3 Pow(decimal n, decimal3 pow) => new decimal3(
            DecimalExtensions.Pow(n, pow.X),
            DecimalExtensions.Pow(n, pow.Y),
            DecimalExtensions.Pow(n, pow.Z));

        public static decimal3 Pow(decimal3 n, decimal3 pow) => new decimal3(
            DecimalExtensions.Pow(n.X, pow.X),
            DecimalExtensions.Pow(n.Y, pow.Y),
            DecimalExtensions.Pow(n.Z, pow.Z));

        public static decimal3 Sqrt(decimal3 n) => new decimal3(
            DecimalExtensions.Pow(n.X, 0.5m),
            DecimalExtensions.Pow(n.Y, 0.5m),
            DecimalExtensions.Pow(n.Z, 0.5m));

        public static decimal3 Sin(decimal3 n) => new decimal3(
            (decimal)Math.Sin((double)n.X),
            (decimal)Math.Sin((double)n.Y),
            (decimal)Math.Sin((double)n.Z));

        public static decimal3 Asin(decimal3 n) => new decimal3(
            (decimal)Math.Asin((double)n.X),
            (decimal)Math.Asin((double)n.Y),
            (decimal)Math.Asin((double)n.Z));

        public static decimal3 Cos(decimal3 n) => new decimal3(
            (decimal)Math.Cos((double)n.X),
            (decimal)Math.Cos((double)n.Y),
            (decimal)Math.Cos((double)n.Z));

        public static decimal3 Acos(decimal3 n) => new decimal3(
            (decimal)Math.Acos((double)n.X),
            (decimal)Math.Acos((double)n.Y),
            (decimal)Math.Acos((double)n.Z));

        public static decimal3 Tan(decimal3 n) => new decimal3(
            (decimal)Math.Tan((double)n.X),
            (decimal)Math.Tan((double)n.Y),
            (decimal)Math.Tan((double)n.Z));

        public static decimal3 Atan(decimal3 n) => new decimal3(
            (decimal)Math.Atan((double)n.X),
            (decimal)Math.Atan((double)n.Y),
            (decimal)Math.Atan((double)n.Z));

        private static decimal Abs(decimal n) => n < 0 ? -n : n;

        public static decimal3 Abs(decimal3 n) => new decimal3(Abs(n.X), Abs(n.Y), Abs(n.Z));

        public static decimal3 Floor(decimal3 n) => new decimal3(
            decimal.Floor(n.X),
            decimal.Floor(n.Y),
            decimal.Floor(n.Z));

        public static decimal3 Ceiling(decimal3 n) => new decimal3(
            decimal.Ceiling(n.X),
            decimal.Ceiling(n.Y),
            decimal.Ceiling(n.Z));

        public static decimal3 Round(decimal3 n) => new decimal3(
            decimal.Round(n.X),
            decimal.Round(n.Y),
            decimal.Round(n.Z));

        public decimal3 Cross(decimal3 n) => new decimal3(
            Y * n.Z - Z * n.Y,
            Z * n.X - X * n.Z,
            X * n.Y - Y * n.X);

        public decimal Dot(decimal3 n) => X * n.X + Y * n.Y + Z * n.Z;
    }
}
