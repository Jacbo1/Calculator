// This class is designed for accuracy over speed so some methods may not be as fast as they could be while losing accuracy

using System;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Calculator
{
    public struct Fraction
    {
        public BigInteger Numerator, Denominator;

        private const double DEG2RAD = Math.PI / 180;
        private const double RAD2DEG = 180 / Math.PI;
        private const int DEFAULT_DECIMAL_COUNT = 100;
        private const int POWER_STEPS = int.MaxValue;
		private const int POW_MAX_DIGITS = 5000;

        private static readonly Fraction POW_EPSILON = new Fraction(1, BigInteger.Pow(10, 100));
        public static readonly Fraction E = (Fraction)Parse("2.71828182845904523536028747135266249775724709369995");
        public static readonly Fraction PI = (Fraction)Parse("3.14159265358979323846264338327950288419716939937510");
		public static readonly Fraction Frac_deg2rad = PI / 180;
        public static readonly Fraction frac_rad2deg = 180 / PI;
		public static readonly Fraction Half = new Fraction(1, 2);

        // Constructors
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
            if (TryParse(x.ToString("F99"), out Fraction? fracNull))
            {
                Fraction frac = (Fraction)fracNull;
                Numerator = frac.Numerator;
                Denominator = frac.Denominator;
            }
            else throw new FractionDoubleParsingException($"{x} is too small or large.");
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
        public static Fraction? Parse(string s)
        {
			Match match = Matching.RE_Number.Match(s);
			if (match.Success)
			{
				// In the form of 12.34
				int dot = s.IndexOf('.');
				if (dot == -1) return new Fraction(BigInteger.Parse(s), BigInteger.One);
				else
				{
					BigInteger denom = BigInteger.Pow(10, s.Length - dot - 1);
					BigInteger num = BigInteger.Parse(s.Substring(0, dot) + s.Substring(dot + 1));
					return new Fraction(num, denom);
				}
			}

			// Check if it is a hex number
			match = Matching.RE_Hex.Match(s);
            if (match.Success)
            {
                // Hex number
                return new Fraction(Convert.ToInt64(match.Value, 16), BigInteger.One);
            }

            // Check if it is a binary number
            match = Matching.RE_Binary.Match(s);
            if (match.Success)
            {
                // Binary number
                return new Fraction(Convert.ToInt64(match.Value, 2), BigInteger.One);
            }

            match = Matching.RE_SciNotation.Match(s);
            if (match.Success)
            {
                // In the form of 1.2E34
                return Parse(match.Groups[1].Value) * Pow(10, (Fraction)Parse(match.Groups[3].Value));
            }

            match = Matching.RE_Fraction.Match(s);
            if (match.Success)
            {
                // In the form of 1.2 / 3.4
                return Parse(match.Groups[1].Value) / Parse(match.Groups[5].Value);
            }

            return null;
        }

        public static bool TryParse(string s, out Fraction? result)
        {
            result = Parse(s);
            return result != null;
        }

        public Fraction Simplify()
        {
            if (Numerator == 0) return new Fraction(0, 1);

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
            return num <= 3 || ((BigInteger)BigInteger.Log10(BigInteger.Abs(num)) + BigInteger.One) * BigInteger.Abs(exp) <= POW_MAX_DIGITS;
        }

        public static Fraction NthRoot(BigInteger x, BigInteger n)
        {
            if (x == 0) return 0;
            if (x < 0) throw new ArithmeticException("Attempted to take a root of a negative number.");

            Fraction pre = new Fraction(BigInteger.One, BigInteger.One);
            Fraction diff = POW_EPSILON;
            Fraction result = new Fraction(BigInteger.Zero, BigInteger.One);
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
                else return Math.Pow((double)x, 1.0 / (double)n); // Too big
			}

            return result;
        }

        public static Fraction Pow(Fraction frac, BigInteger exponent)
        {
            // Exponent is whole number
            if (exponent == 0) return new Fraction(1, 1);

            // 2 common exponents to avoid the digit check resulting in precision loss
			if (exponent == 2) return frac * frac;
            if (exponent == 3) return frac * frac * frac;
            if (!CheckPowDigits(frac.Numerator, exponent) || !CheckPowDigits(frac.Denominator, exponent))
            {
                // Too big
                return Math.Pow((double)frac, (double)exponent);
            }

            BigInteger abs = BigInteger.Abs(exponent);
            BigInteger num = BigInteger.One;
            BigInteger denom = BigInteger.One;
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

            return exponent > 0 ? new Fraction(num, denom) : new Fraction(denom, num);
        }

        public static Fraction Pow(Fraction frac, Fraction exponent)
        {
            if (exponent.Denominator == 1)
            {
                // Exponent is whole number
                return Pow(frac, exponent.Numerator);
            }

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

            if (!CheckPowDigits(numPow, exponent.Denominator) || !CheckPowDigits(denomPow, exponent.Denominator))
            {
                // Too big
                return Math.Pow((double)frac, (double)exponent);
            }

            Fraction numRoot = NthRoot(numPow, exponent.Denominator);
            Fraction denomRoot = NthRoot(denomPow, exponent.Denominator);

            return Pow(frac, wholePower) * (exponent.Numerator > 0 ? numRoot / denomRoot : denomRoot / numRoot);
        }

        public static Fraction Round(Fraction frac)
        {
            if (frac.Denominator == 1) return frac; // Whole number
			if (frac.Numerator < 0) return Ceiling(frac - Half); // Negative
			return Floor(frac + Half);
        }

        public static Fraction Round(Fraction frac, int digits)
        {
            if (frac.Denominator == 1) return frac; // Whole number
			BigInteger mult = BigInteger.Pow(10, digits);
            if (frac.Numerator < 0) return Ceiling(frac * mult - Half) / mult; // Negative
			return Floor(frac * mult + Half) / mult;
        }

        public static Fraction Round(Fraction frac, Fraction digits) => Round(frac, (int)digits);

        public static Fraction Min(Fraction a, Fraction b) => a < b ? a : b;

        public static Fraction Max(Fraction a, Fraction b) => a > b ? a : b;

        public static Fraction Floor(Fraction frac)
        {
            if (frac.Numerator < 0)
            {
                BigInteger floored = BigInteger.DivRem(frac.Numerator, frac.Denominator, out BigInteger rem);
                return rem == 0 ? floored : floored - 1;
            }

            return frac.Numerator / frac.Denominator;
        }

        public static Fraction Ceiling(Fraction frac)
        {
            if (frac.Numerator < 0) return frac.Numerator / frac.Denominator;

            BigInteger floored = BigInteger.DivRem(frac.Numerator, frac.Denominator, out BigInteger rem);
            return rem == 0 ? floored : floored + 1;
        }

        public static int Sign(Fraction frac) => frac.Numerator.Sign;


        // ToString
        public string ToString(int decimalCount)
        {
            if (Denominator == 1) return Numerator.ToString(); // Whole number
            if (decimalCount == 0)
            {
                // No decimals
                return Matching.RE_TrailingZeroes.Replace((Numerator / Denominator).ToString(), "");
            }

            // Decimal
            string s = (BigInteger.Pow(10, decimalCount) * BigInteger.Abs(Numerator) / Denominator).ToString();
            if (s.Length <= decimalCount) s = new string('0', decimalCount - s.Length + 1) + s;

            if (Numerator < 0) s = '-' + s;
            s = s.Substring(0, s.Length - decimalCount) + "." + s.Substring(s.Length - decimalCount);

            return Matching.RE_TrailingZeroes.Replace(s, "");
        }

        public string ToFracString() => $"{Numerator} / {Denominator}";

        public override string ToString() => ToString(DEFAULT_DECIMAL_COUNT);

        public string ToMinString()
        {
            if (Denominator == 1) return Numerator.ToString();
            return $"{Numerator} / {Denominator}";
        }

        // Trig
        public static Fraction Sin(Fraction frac) => Math.Sin((double)(frac % 360) * DEG2RAD);

        public static Fraction Cos(Fraction frac) => Math.Cos((double)(frac % 360) * DEG2RAD);

        public static Fraction Tan(Fraction frac) => Math.Tan((double)(frac % 360) * DEG2RAD);

        public static Fraction Asin(Fraction frac) => Math.Asin((double)frac) * RAD2DEG;

        public static Fraction Acos(Fraction frac) => Math.Acos((double)frac) * RAD2DEG;

        public static Fraction Atan(Fraction frac) => Math.Atan((double)frac) * RAD2DEG;

        public static Fraction Atan2(Fraction y, Fraction x) => Math.Atan2((double)y, (double)x) * RAD2DEG;

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
            a.Denominator * b.Denominator
        );

        public static Fraction operator -(Fraction a, Fraction b) => new Fraction(
            a.Numerator * b.Denominator - b.Numerator * a.Denominator,
            a.Denominator * b.Denominator
        );

        public static Fraction operator -(Fraction a) => new Fraction(-a.Numerator, a.Denominator);

        public static Fraction operator *(Fraction a, Fraction b) => new Fraction(
            a.Numerator * b.Numerator,
            a.Denominator * b.Denominator
        );

        public static Fraction operator /(Fraction a, Fraction b) => new Fraction(
            a.Numerator * b.Denominator,
            a.Denominator * b.Numerator
        );

        public static Fraction operator %(Fraction a, Fraction b) => new Fraction(
            (a.Numerator * b.Denominator) % (b.Numerator * a.Denominator),
            a.Denominator * b.Denominator
        );

        // Comparison
        public static bool operator ==(Fraction a, Fraction b) => a.Numerator == b.Numerator && a.Denominator == b.Denominator;

        public static bool operator !=(Fraction a, Fraction b) => a.Numerator != b.Numerator || a.Denominator != b.Denominator;

        public static bool operator ==(Fraction a, object b)
        {
            if (b is int b1) return a.Denominator == 1 && a.Numerator == b1;
            if (b is long b2) return a.Denominator == 1 && a.Numerator == b2;
            if (b is double b3)
            {
                Fraction? frac = Parse(b3.ToString());
                return a.Numerator == frac?.Numerator && a.Denominator == frac?.Denominator;
            }
            return false;
        }

        public static bool operator ==(object a, Fraction b)
        {
            if (a is int a1) return b.Denominator == 1 && b.Numerator == a1;
            if (a is long a2) return b.Denominator == 1 && b.Numerator == a2;
            if (a is double a3)
            {
                Fraction? frac = Parse(a3.ToString());
                return b.Numerator == frac?.Numerator && b.Denominator == frac?.Denominator;
            }
            return false;
        }

        public static bool operator !=(Fraction a, object b)
        {
            if (b is int b1) return a.Denominator != 1 || a.Numerator != b1;
            if (b is long b2) return a.Denominator != 1 || a.Numerator != b2;
            if (b is double b3)
            {
                Fraction? frac = Parse(b3.ToString());
                return a.Numerator != frac?.Numerator || a.Denominator != frac?.Denominator;
            }
            return false;
        }

        public static bool operator !=(object a, Fraction b)
        {
            if (a is int a1) return b.Denominator != 1 || b.Numerator != a1;
            if (a is long a2) return b.Denominator != 1 || b.Numerator != a2;
            if (a is double a3)
            {
                Fraction? frac = Parse(a3.ToString());
                return b.Numerator != frac?.Numerator || b.Denominator != frac?.Denominator;
            }
            return false;
        }

        public override bool Equals(object o)
        {
            if (o is Fraction b) return Numerator == b.Numerator && Denominator == b.Denominator;
            return false;
        }

        public static bool operator >(Fraction a, Fraction b) => a.Numerator * b.Denominator > b.Numerator * a.Denominator;

        public static bool operator <(Fraction a, Fraction b) => a.Numerator * b.Denominator < b.Numerator * a.Denominator;

        public static bool operator >=(Fraction a, Fraction b) => a.Numerator * b.Denominator >= b.Numerator * a.Denominator;

        public static bool operator <=(Fraction a, Fraction b) => a.Numerator * b.Denominator <= b.Numerator * a.Denominator;
    }
}
