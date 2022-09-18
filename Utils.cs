using System;

namespace Calculator
{
    public static class Utils
    {
        public static Vector Op(Vector vec, Func<Fraction, Fraction> func) => new Vector(func(vec.X), func(vec.Y), func(vec.Z));
        public static Vector Op(Vector a, Vector b, Func<Fraction, Fraction, Fraction> func) => new Vector(func(a.X, b.X), func(a.Y, b.Y), func(a.Z, b.Z));
        public static Vector Op(Vector a, Fraction b, Func<Fraction, Fraction, Fraction> func) => new Vector(func(a.X, b), func(a.Y, b), func(a.Z, b));
        public static Vector Op(Fraction a, Vector b, Func<Fraction, Fraction, Fraction> func) => new Vector(func(a, b.X), func(a, b.Y), func(a, b.Z));
        public static Vector Op(Vector a, int b, Func<Fraction, int, Fraction> func) => new Vector(func(a.X, b), func(a.Y, b), func(a.Z, b));
        public static Vector Op(Vector a, Vector b, Func<Fraction, int, Fraction> func) => new Vector(func(a.X, (int)b.X), func(a.Y, (int)b.Y), func(a.Z, (int)b.Z));
    }
}
