using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Calculator
{
    internal class FormulaGroup
    {
        private static readonly Regex varRegex = new Regex(@"(\S+)=(\S+)", RegexOptions.Compiled);
        private static readonly Regex whitespace = new Regex(@"\s", RegexOptions.Compiled);

        public static string Calculate(string input, out string workOutput, OutputMode outputMode)
        {
            workOutput = "";
            string answer = "";

            Piece answerPiece = new Piece("");

            Dictionary<string, Piece> vars = new Dictionary<string, Piece>();
            foreach (string line2 in input.Split('\n'))
            {
                string line = whitespace.Replace(line2, "");
                if (line.Length > 0 && line[0] == ';') continue; // Skip commented line

                Match match = varRegex.Match(line);
                if (match.Success)
                {
                    // This line is a variable assignment
                    string varName = match.Groups[1].Value;
                    string right = match.Groups[2].Value;
                    answer = Matching.Answer2Decimal(new Formula(right, vars).Calculate(out string work, false, false, false, out answerPiece));
                    work = work.Trim();
                    Piece varPiece = new Piece(answer)
                    {
                        IsVar = true,
                        VarName = varName
                    };

                    vars[varName] = varPiece;

                    if (work.Length > 0 && !Matching.RE_GenericNum.IsMatch(right))
                    {
                        workOutput += $"{work}\n{answer}\n\n";
                    }
                }
                else if (line.Length > 0)
                {
                    answer = Matching.Answer2Decimal(new Formula(line, vars).Calculate(out string work, false, true, false, out answerPiece));
                    work = work.Trim();
                    workOutput += $"{work}\n{answer}\n\n";
                }
            }

            workOutput = workOutput.Trim();

            try
            {
                switch (outputMode)
                {
                    case OutputMode.Scientific:
                        // Output in scientific notation
                        const int DEC_DIGIT_DISPLAY = 4;

                        string ConvertToScientific(Fraction n)
                        {
                            if (n == 0) return "0.0";

                            int digits;
                            if (n.Numerator >= n.Denominator) digits = (int)Math.Floor(0.000005 + BigInteger.Log10(BigInteger.Abs(n.Numerator / n.Denominator)));
                            else
                            {
                                const int DIGIT_SHIFT = 1000;
                                BigInteger mult = BigInteger.Pow(10, DIGIT_SHIFT);
                                Fraction big = n * mult;
                                digits = (int)Math.Floor(0.000005 + BigInteger.Log10(BigInteger.Abs(big.Numerator / big.Denominator))) - DIGIT_SHIFT;
                            }

                            Fraction sci = n * Fraction.Pow(10, -digits);
                            sci = Fraction.Round(sci, DEC_DIGIT_DISPLAY);
                            string s = sci.ToString(DEC_DIGIT_DISPLAY);
                            if (s.IndexOf('.') == -1) s += ".0";
                            return s + "E" + digits;
                        };

                        switch (answerPiece.Type)
                        {
                            case "num": return ConvertToScientific((Fraction)answerPiece.Value);
                            case "const": return ConvertToScientific((Fraction)answerPiece.ConstValue);
                            case "vec":
                                Vector v = (Vector)answerPiece.Value;
                                return $"<{ConvertToScientific(v.X)}, {ConvertToScientific(v.Y)}, {ConvertToScientific(v.Z)}>";
                        }
                        break;
                    case OutputMode.Fractions:
                        // Output in fraction notation
                        string ToFrac(Fraction f)
                        {
                            if (f.Denominator == 1) return f.Numerator.ToString();
                            return $"{f.Numerator} / {f.Denominator}";
                        }

                        switch (answerPiece.Type)
                        {
                            case "num": return ToFrac((Fraction)answerPiece.Value);
                            case "const": return ToFrac((Fraction)answerPiece.ConstValue);
                            case "vec":
                                Vector v = (Vector)answerPiece.Value;
                                return $"<{ToFrac(v.X)}, {ToFrac(v.Y)}, {ToFrac(v.Z)}>";
                        }
                        break;
                    case OutputMode.Binary:
                        switch (answerPiece.Type)
                        {
                            case "num": return ToBinaryString((BigInteger)(Fraction)answerPiece.Value);
                            case "const": return ToBinaryString((BigInteger)answerPiece.ConstValue);
                            case "vec":
                                Vector v = (Vector)answerPiece.Value;
                                return $"<{ToBinaryString((BigInteger)v.X)}, {ToBinaryString((BigInteger)v.Y)}, {ToBinaryString((BigInteger)v.Z)}>";
                        }
                        break;
                    case OutputMode.Hex:
                        switch (answerPiece.Type)
                        {
                            case "num": return ToHexString((BigInteger)(Fraction)answerPiece.Value);
                            case "const": return ToHexString((BigInteger)answerPiece.ConstValue);
                            case "vec":
                                Vector v = (Vector)answerPiece.Value;
                                return $"<{ToHexString((BigInteger)v.X)}, {ToHexString((BigInteger)v.Y)}, {ToHexString((BigInteger)v.Z)}>";
                        }
                        break;
                    case OutputMode.ColorHex:
                        if (answerPiece.Type == "vec")
                        {
                            //return "#"
                            string ToHex(Fraction n)
                            {
                                string s = BigInteger.Min(BigInteger.Max(Fraction.Round(n).Numerator, 0), 255).ToString("X2");
                                if (s.Length > 2) return s.Substring(1, 2);
                                return s;
                            }

                            Vector v = (Vector)answerPiece.Value;
                            return $"#{ToHex(v.X)}{ToHex(v.Y)}{ToHex(v.Z)}";
                        }
                        break;
                }
            }
            catch { }

            return answer;
        }

        private static string ToHexString(BigInteger bigint)
        {
            string str = BigInteger.Abs(bigint).ToString("X");

            if (str.Length % 2 == 1)
            {
                if (str[0] == '0') str = str.Substring(1);
                else str = '0' + str;
            }

            for (int i = str.Length - 2; i >= 2; i -= 2)
            {
                str = str.Substring(0, i) + ' ' + str.Substring(i);
            }

            return "0x" + str;
        }

        private static string ToBinaryString(BigInteger bigint)
        {
			byte[] bytes = BigInteger.Abs(bigint).ToByteArray();
			int idx = bytes.Length - 1;

			// Create a StringBuilder having appropriate capacity.
			System.Text.StringBuilder base2 = new System.Text.StringBuilder(bytes.Length * 8);

			// Convert first byte to binary.
			string binary = Convert.ToString(bytes[idx], 2);

            // Append binary string to StringBuilder.
            base2.Append(binary);

            // Convert remaining bytes adding leading zeros.
            for (idx--; idx >= 0; idx--)
            {
                base2.Append(Convert.ToString(bytes[idx], 2).PadLeft(8, '0'));
            }

            string str = base2.ToString();
            str = str.Substring(Math.Max(0, str.IndexOf('1')));
            if (str.Length % 8 != 0) str = new string('0', 8 - str.Length % 8) + str;

            for (int i = str.Length - 4; i >= 4; i -= 4)
            {
                str = str.Substring(0, i) + ' ' + str.Substring(i);
            }

            return "0b" + str;
        }
    }
}
