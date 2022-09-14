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

        public static string Calculate(string input, out string workOutput, Enums.OutputMode outputMode)
        {
            workOutput = "";
            string answer = "";

            Piece answerPiece = new Piece("");

            Dictionary<string, Piece> vars = new Dictionary<string, Piece>();
            foreach (string line2 in input.Split('\n'))
            {
                string line = whitespace.Replace(line2, "");
                if (line.Length > 0 && line[0] == ';')
                {
                    // Skip commented line
                    continue;
                }
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
                    case Enums.OutputMode.Scientific:
                        // Output in scientific notation
                        const int DEC_DIGIT_DISPLAY = 4;

                        Func<Fraction, string> convertToScientific = n =>
                        {
                            if (n == 0)
                                return "0.0";

                            int digits;
                            if (n.Numerator >= n.Denominator)
                            {
                                digits = (int)Math.Floor(0.000005 + BigInteger.Log10(BigInteger.Abs(n.Numerator / n.Denominator)));
                            }
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
                            if (s.IndexOf('.') == -1)
                            {
                                s += ".0";
                            }

                            return s + "E" + digits;
                        };

                        switch (answerPiece.Type)
                        {
                            case "num":
                                return convertToScientific((Fraction)answerPiece.Value);
                            case "const":
                                return convertToScientific(answerPiece.ConstValue);
                            case "vec":
                                Vector v = (Vector)answerPiece.Value;
                                return $"<{convertToScientific(v.X)}, {convertToScientific(v.Y)}, {convertToScientific(v.Z)}>";
                        }
                        break;
                    case Enums.OutputMode.Fractions:
                        // Output in fraction notation
                        switch (answerPiece.Type)
                        {
                            case "num":
                                return ((Fraction)answerPiece.Value).ToFracString();
                            case "const":
                                return answerPiece.ConstValue.ToFracString();
                            case "vec":
                                Vector v = (Vector)answerPiece.Value;
                                return $"<{v.X.ToFracString()}, {v.Y.ToFracString()}, {v.Z.ToFracString()}>";
                        }
                        break;
                    case Enums.OutputMode.Binary:
                        switch (answerPiece.Type)
                        {
                            case "num":
                                return ToBinaryString((BigInteger)(Fraction)answerPiece.Value);
                            case "const":
                                return ToBinaryString((BigInteger)answerPiece.ConstValue);
                            case "vec":
                                Vector v = (Vector)answerPiece.Value;
                                return $"<{ToBinaryString((BigInteger)v.X)}, {ToBinaryString((BigInteger)v.Y)}, {ToBinaryString((BigInteger)v.Z)}>";
                        }
                        break;
                    case Enums.OutputMode.Hex:
                        switch (answerPiece.Type)
                        {
                            case "num":
                                return ToHexString((BigInteger)(Fraction)answerPiece.Value);
                            case "const":
                                return ToHexString((BigInteger)answerPiece.ConstValue);
                            case "vec":
                                Vector v = (Vector)answerPiece.Value;
                                return $"<{ToHexString((BigInteger)v.X)}, {ToHexString((BigInteger)v.Y)}, {ToHexString((BigInteger)v.Z)}>";
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
                if (str[0] == '0')
                    str = str.Substring(1);
                else
                    str = '0' + str;

            for (int i = str.Length - 2; i >= 2; i -= 2)
                str = str.Substring(0, i) + ' ' + str.Substring(i);

            return "0x" + str;
        }

        private static string ToBinaryString(BigInteger bigint)
        {
            var bytes = BigInteger.Abs(bigint).ToByteArray();
            var idx = bytes.Length - 1;

            // Create a StringBuilder having appropriate capacity.
            var base2 = new System.Text.StringBuilder(bytes.Length * 8);

            // Convert first byte to binary.
            var binary = Convert.ToString(bytes[idx], 2);

            // Append binary string to StringBuilder.
            base2.Append(binary);

            // Convert remaining bytes adding leading zeros.
            for (idx--; idx >= 0; idx--)
            {
                base2.Append(Convert.ToString(bytes[idx], 2).PadLeft(8, '0'));
            }

            string str = base2.ToString();
            str = str.Substring(Math.Max(0, str.IndexOf('1')));
            if (str.Length % 8 != 0)
                str = new string('0', 8 - str.Length % 8) + str;

            for (int i = str.Length - 4; i >= 4; i -= 4)
                str = str.Substring(0, i) + ' ' + str.Substring(i);

            return "0b" + str;
        }
    }
}
