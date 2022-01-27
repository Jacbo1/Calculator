// This class is designed for accuracy over speed so some methods may not be as fast as they could be while losing accuracy

using System;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Calculator
{
    internal class Fraction
    {
        public BigInteger Numerator, Denominator;

        private static readonly Fraction POW_EPSILON = new Fraction(1, BigInteger.Pow(10, 100));
        public static readonly Fraction E = Parse("2.71828182845904523536028747135266249775724709369995"),
            PI = Parse("3.14159265358979323846264338327950288419716939937510");
        private const double deg2rad = Math.PI / 180,
            rad2deg = 180 / Math.PI;
        private const int DEFAULT_DECIMAL_COUNT = 100,
            POWER_STEPS = int.MaxValue,
            POW_MAX_DIGITS = 10000;
        private readonly static Fraction HALF = new Fraction(1, 2);

        // Constructors
        public Fraction()
        {
            Numerator = BigInteger.Zero;
            Denominator = BigInteger.One;
        }

        public Fraction(long x)
        {
            Numerator = x;
            Denominator = BigInteger.One;
        }

        public Fraction(long num, long denom)
        {
            if (denom < 0)
            {
                Numerator = -num;
                Denominator = -denom;
            }
            else
            {
                Numerator = num;
                Denominator = denom;
            }

            // Simplify
            BigInteger gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(Numerator), Denominator);
            if (gcd > 1)
            {
                Numerator /= gcd;
                Denominator /= gcd;
            }
        }

        public Fraction(double x)
        {
            if (TryParse(x.ToString("F99").Trim('0'), out Fraction frac))
            {
                Numerator = frac.Numerator;
                Denominator = frac.Denominator;
            }
            else
            {
                throw new FractionDoubleParsingException();
            }
        }

        public Fraction(BigInteger x)
        {
            Numerator = x;
            Denominator = BigInteger.One;
        }

        public Fraction(BigInteger num, BigInteger denom)
        {
            if (denom < 0)
            {
                Numerator = -num;
                Denominator = -denom;
            }
            else
            {
                Numerator = num;
                Denominator = denom;
            }

            // Simplify
            BigInteger gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(Numerator), Denominator);
            if (gcd > 1)
            {
                Numerator /= gcd;
                Denominator /= gcd;
            }
        }


        // Methods
        public static Fraction Parse(string s)
        {
            Match match = Regexes.RE_Number.Match(s);
            if (match.Success)
            {
                // In the form of 12.34
                int dot = s.IndexOf('.');
                if (dot == -1)
                {
                    return new Fraction(
                        BigInteger.Parse(s),
                        BigInteger.One);
                }
                else
                {
                    BigInteger denom = BigInteger.Pow(10, s.Length - dot - 1);
                    BigInteger num = BigInteger.Parse(s.Substring(0, dot) + s.Substring(dot + 1));
                    return new Fraction(num, denom);
                }
            }

            match = Regexes.RE_SciNotation.Match(s);
            if (match.Success)
            {
                // In the form of 1.2E34
                return Parse(match.Groups[1].Value) * Pow(10, Parse(match.Groups[3].Value));
            }

            match = Regexes.RE_Fraction.Match(s);
            if (match.Success)
            {
                // In the form of 1.2 / 3.4
                return Parse(match.Groups[1].Value) / Parse(match.Groups[5].Value);
            }

            return null;
        }

        public static bool TryParse(string s, out Fraction result)
        {
            result = Parse(s);
            return !(result is null);
        }

        public Fraction Clone() => new Fraction(Numerator, Denominator);

        public Fraction Simplify()
        {
            if (Numerator == 0)
            {
                return new Fraction(0, 1);
            }
            BigInteger gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(Numerator), Denominator);
            if (gcd > 1)
            {
                BigInteger num = Numerator / gcd;
                BigInteger denom = Denominator / gcd;
                return new Fraction(num, denom);
            }
            return this;
        }

        public static Fraction Abs(Fraction num) => new Fraction(BigInteger.Abs(num.Numerator), num.Denominator);

        private static bool CheckPowDigits(BigInteger num, BigInteger exp)
        {
            // Rough estimate
            if (num <= 3)
            {
                return true;
            }
            return ((BigInteger)BigInteger.Log10(BigInteger.Abs(num)) + BigInteger.One) * BigInteger.Abs(exp) <= POW_MAX_DIGITS;
        }

        public static Fraction NthRoot(BigInteger x, BigInteger n)
        {
            if (x == 0)
            {
                return 0;
            }
            if (x < 0)
            {
                throw new ArithmeticException("Attempted to take a root of a negative number.");
            }
            Fraction pre = new Fraction(1, 1);
            Fraction diff = POW_EPSILON.Clone();
            Fraction result = new Fraction(0, 1);
            BigInteger n1 = n - 1;
            Fraction n1frac = n1;
            Fraction nFrac = n;
            Fraction xFrac = x;

            while (diff >= POW_EPSILON)
            {
                if (CheckPowDigits(pre.Numerator, n1) && CheckPowDigits(pre.Denominator, n1))
                {
                    result = (n1frac * pre + xFrac / Pow(pre, n1)) / nFrac;
                    diff = Abs(result - pre);
                    pre = result;
                }
                else
                {
                    // Too big
                    return Math.Pow((double)x, 1.0 / (double)n);
                }
            }

            return result;
        }

        public static Fraction Pow(Fraction frac, BigInteger exponent)
        {
            // Exponent is whole number
            if (exponent == 0)
            {
                // 0
                return new Fraction(1, 1);
            }

            if (!CheckPowDigits(frac.Numerator, exponent) || !CheckPowDigits(frac.Denominator, exponent))
            {
                // Too big
                return Math.Pow((double)frac, (double)exponent);
            }

            BigInteger abs = BigInteger.Abs(exponent);
            BigInteger num = 1;
            BigInteger denom = 1;
            if (abs > POWER_STEPS)
            {
                BigInteger numPow = BigInteger.Pow(frac.Numerator, POWER_STEPS);
                BigInteger denomPow = BigInteger.Pow(frac.Denominator, POWER_STEPS);
                while (abs > POWER_STEPS)
                {
                    num *= numPow;
                    denom *= denomPow;
                    BigInteger gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(num), denom);
                    num /= gcd;
                    denom /= gcd;
                    abs -= POWER_STEPS;
                }
            }
            int exp = (int)abs;
            num *= BigInteger.Pow(frac.Numerator, exp);
            denom *= BigInteger.Pow(frac.Denominator, exp);

            if (exponent > 0)
            {
                return new Fraction(num, denom);
            }
            return new Fraction(denom, num);
        }

        public static Fraction Pow(Fraction frac, Fraction exponent)
        {
            if (exponent.Denominator == 1)
            {
                // Exponent is whole number
                return Pow(frac, exponent.Numerator);
            }
            else
            {
                // Exponent is a fraction
                BigInteger wholePower = BigInteger.DivRem(exponent.Numerator, exponent.Denominator, out BigInteger rem);
                if (!CheckPowDigits(frac.Numerator, rem) || !CheckPowDigits(frac.Denominator, rem))
                {
                    // Too big
                    return Math.Pow((double)frac, (double)exponent);
                }

                int remi = (int)BigInteger.Abs(rem);
                BigInteger numPow = BigInteger.Pow(frac.Numerator, remi);
                BigInteger denomPow = BigInteger.Pow(frac.Denominator, remi);
                Fraction numRoot = NthRoot(numPow, exponent.Denominator);
                Fraction denomRoot = NthRoot(denomPow, exponent.Denominator);

                if (exponent.Numerator > 0)
                {
                    // Positive exponent
                    return Pow(frac, wholePower) * numRoot / denomRoot;
                }
                // Negative exponent
                return Pow(frac, wholePower) * denomRoot / numRoot;
            }
        }

        public static Fraction Round(Fraction frac)
        {
            // This rounds up 0.5 because that's how rounding actually works even though C#'s Math.Round() does not
            if (frac.Denominator == 1)
            {
                // Whole number
                return frac.Clone();
            }

            // Not whole number
            return Fraction.Floor(frac + HALF);
        }

        public static Fraction Round(Fraction frac, int digits)
        {
            // This rounds up 0.5 because that's how rounding actually works even though C#'s Math.Round() does not
            if (frac.Denominator == 1)
            {
                // Whole number
                return frac.Clone();
            }

            // Not whole number
            BigInteger mult = BigInteger.Pow(10, digits);
            return Fraction.Floor(frac * mult + HALF) / mult;
        }

        public static Fraction Min(Fraction a, Fraction b)
        {
            return (a < b ? a : b).Clone();
        }

        public static Fraction Max(Fraction a, Fraction b)
        {
            return (a > b ? a : b).Clone();
        }

        public static Fraction Floor(Fraction frac)
        {
            if (frac.Numerator < 0)
            {
                BigInteger floored = BigInteger.DivRem(frac.Numerator, frac.Denominator, out BigInteger rem);
                if (rem == 0)
                {
                    return floored;
                }
                return floored - 1;
            }
            return frac.Numerator / frac.Denominator;
        }

        public static Fraction Ceiling(Fraction frac)
        {
            if (frac.Numerator < 0)
            {
                return frac.Numerator / frac.Denominator;
            }
            BigInteger floored = BigInteger.DivRem(frac.Numerator, frac.Denominator, out BigInteger rem);
            if (rem == 0)
            {
                return floored;
            }
            return floored + 1;
        }

        public string ToFracString()
        {
            return $"{Numerator} / {Denominator}";
        }

        public string ToString(int decimalCount)
        {
            if (Denominator == 1)
            {
                // Whole number
                return Numerator.ToString();
            }

            if (decimalCount == 0)
            {
                // No decimals
                return Regexes.RE_TrailingZeroes.Replace((Numerator / Denominator).ToString(), "");
            }

            // Decimal
            string s = (BigInteger.Pow(10, decimalCount) * BigInteger.Abs(Numerator) / Denominator).ToString();
            if (s.Length <= decimalCount)
            {
                s = new string('0', decimalCount - s.Length + 1) + s;
            }

            if (Numerator < 0)
            {
                s = '-' + s;
            }
            s = s.Substring(0, s.Length - decimalCount) + "." + s.Substring(s.Length - decimalCount);

            return Regexes.RE_TrailingZeroes.Replace(s, "");
        }

        public override string ToString() => ToString(DEFAULT_DECIMAL_COUNT);

        // Trig
        public static Fraction Sin(Fraction frac)
        {
            return Math.Sin((double)(frac % 360) * rad2deg);
        }

        public static Fraction Cos(Fraction frac)
        {
            return Math.Cos((double)(frac % 360) * rad2deg);
        }

        public static Fraction Tan(Fraction frac)
        {
            return Math.Tan((double)(frac % 360) * rad2deg);
        }

        public static Fraction Asin(Fraction frac)
        {
            return Math.Asin((double)frac) * deg2rad;
        }

        public static Fraction Acos(Fraction frac)
        {
            return Math.Acos((double)frac) * deg2rad;
        }

        public static Fraction Atan(Fraction frac)
        {
            return Math.Atan((double)frac) * deg2rad;
        }

        // Explicits
        public static explicit operator double(Fraction x) => double.Parse(x.ToString(DEFAULT_DECIMAL_COUNT)); // Higher accuracy since it won't be dividing 2 doubles

        public static explicit operator int(Fraction x) => (int)(x.Numerator / x.Denominator);

        public static explicit operator long(Fraction x) => (long)(x.Numerator / x.Denominator);

        public static explicit operator BigInteger(Fraction x) => x.Numerator / x.Denominator;

        // Implicits
        // Conversions
        public static implicit operator Fraction(long x) => new Fraction(x);

        public static implicit operator Fraction(double x) => new Fraction(x);

        public static implicit operator Fraction(BigInteger x) => new Fraction(x);

        // Operators
        public static Fraction operator +(Fraction a, Fraction b) => new Fraction(
            a.Numerator * b.Denominator + b.Numerator * a.Denominator,
            a.Denominator * b.Denominator).Simplify();

        public static Fraction operator -(Fraction a, Fraction b) => new Fraction(
            a.Numerator * b.Denominator - b.Numerator * a.Denominator,
            a.Denominator * b.Denominator).Simplify();

        public static Fraction operator -(Fraction a) => new Fraction(-a.Numerator, a.Denominator);

        public static Fraction operator *(Fraction a, Fraction b) => new Fraction(
            a.Numerator * b.Numerator,
            a.Denominator * b.Denominator).Simplify();

        public static Fraction operator /(Fraction a, Fraction b) => new Fraction(
            a.Numerator * b.Denominator,
            a.Denominator * b.Numerator).Simplify();

        public static Fraction operator %(Fraction a, Fraction b) => new Fraction(
            (a.Numerator * b.Denominator) % (b.Numerator * a.Denominator),
            a.Denominator * b.Denominator).Simplify();

        // Comparison
        public static bool operator ==(Fraction a, Fraction b) => a.Numerator == b.Numerator && a.Denominator == b.Denominator;

        public static bool operator !=(Fraction a, Fraction b) => a.Numerator != b.Numerator || a.Denominator != b.Denominator;

        public static bool operator ==(Fraction a, object b)
        {
            if (b is int b1)
            {
                return a.Denominator == 1 && a.Numerator == b1;
            }
            if (b is long b2)
            {
                return a.Denominator == 1 && a.Numerator == b2;
            }
            if (b is double b3)
            {
                Fraction frac = Parse(b3.ToString());
                return a.Numerator == frac.Numerator && a.Denominator == frac.Denominator;
            }
            return false;
        }

        public static bool operator ==(object a, Fraction b)
        {
            if (a is int a1)
            {
                return b.Denominator == 1 && b.Numerator == a1;
            }
            if (a is long a2)
            {
                return b.Denominator == 1 && b.Numerator == a2;
            }
            if (a is double a3)
            {
                Fraction frac = Parse(a3.ToString());
                return b.Numerator == frac.Numerator && b.Denominator == frac.Denominator;
            }
            return false;
        }

        public static bool operator !=(Fraction a, object b)
        {
            if (b is int b1)
            {
                return a.Denominator != 1 || a.Numerator != b1;
            }
            if (b is long b2)
            {
                return a.Denominator != 1 || a.Numerator != b2;
            }
            if (b is double b3)
            {
                Fraction frac = Parse(b3.ToString());
                return a.Numerator != frac.Numerator || a.Denominator != frac.Denominator;
            }
            return false;
        }

        public static bool operator !=(object a, Fraction b)
        {
            if (a is int a1)
            {
                return b.Denominator != 1 || b.Numerator != a1;
            }
            if (a is long a2)
            {
                return b.Denominator != 1 || b.Numerator != a2;
            }
            if (a is double a3)
            {
                Fraction frac = Parse(a3.ToString());
                return b.Numerator != frac.Numerator || b.Denominator != frac.Denominator;
            }
            return false;
        }

        public override bool Equals(object o)
        {
            if (o is Fraction b)
            {
                return Numerator == b.Numerator && Denominator == b.Denominator;
            }
            return false;
        }

        public static bool operator >(Fraction a, Fraction b) => a.Numerator * b.Denominator > b.Numerator * a.Denominator;

        public static bool operator <(Fraction a, Fraction b) => a.Numerator * b.Denominator < b.Numerator * a.Denominator;

        public static bool operator >=(Fraction a, Fraction b) => a.Numerator * b.Denominator >= b.Numerator * a.Denominator;

        public static bool operator <=(Fraction a, Fraction b) => a.Numerator * b.Denominator <= b.Numerator * a.Denominator;
    }
}
