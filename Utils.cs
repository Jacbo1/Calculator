using System;

namespace Calculator
{
    internal static class Utils
    {
        public static Fraction Op(Fraction a, Fraction b, Func<Fraction, Fraction, Fraction> func) => func(a, b);
        public static Fraction Op(Fraction a, Func<Fraction, Fraction> func) => func(a);
        public static Vector Op(Vector vec, Func<Fraction, Fraction> func) => new Vector(func(vec.X), func(vec.Y), func(vec.Z));
        public static Vector Op(Vector a, Vector b, Func<Fraction, Fraction, Fraction> func) => new Vector(func(a.X, b.X), func(a.Y, b.Y), func(a.Z, b.Z));
        public static Vector Op(Vector a, Fraction b, Func<Fraction, Fraction, Fraction> func) => new Vector(func(a.X, b), func(a.Y, b), func(a.Z, b));
        public static Vector Op(Fraction a, Vector b, Func<Fraction, Fraction, Fraction> func) => new Vector(func(a, b.X), func(a, b.Y), func(a, b.Z));
        public static Vector Op(Vector a, int b, Func<Fraction, int, Fraction> func) => new Vector(func(a.X, b), func(a.Y, b), func(a.Z, b));
        public static Vector Op(Vector a, Vector b, Func<Fraction, int, Fraction> func) => new Vector(func(a.X, (int)b.X), func(a.Y, (int)b.Y), func(a.Z, (int)b.Z));
        public static Piece Op(Piece a, Func<Fraction, Fraction> func)
        {
            switch (a.Type)
            {
                case "num": return new Piece(Op((Fraction)a.Value, func));
                case "vec": return new Piece(Op((Vector)a.Value, func));
                default: throw new Exception();
            }
        }

        public static Piece Op(Piece a, Piece b, Func<Fraction, Fraction, Fraction> func)
        {
            switch (a.Type + b.Type)
            {
                case "numnum": return new Piece(Op((Fraction)a.Value, (Fraction)b.Value, func));
                case "numvec": return new Piece(Op((Fraction)a.Value, (Vector)b.Value, func));
                case "vecnum": return new Piece(Op((Vector)a.Value, (Fraction)b.Value, func));
                case "vecvec": return new Piece(Op((Vector)a.Value, (Vector)b.Value, func));
                default: throw new Exception();
            }
        }
    }
}
