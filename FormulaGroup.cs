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

        public static string Calculate(string input, out string workOutput, bool showScientific, bool showFractions)
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
                if (showScientific)
                {
                    // Output in scientific notation
                    const int DEC_DIGIT_DISPLAY = 4;

                    Func<Fraction, string> convertToScientific = n =>
                    {
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

                    if (answerPiece.Type.Equals("num"))
                    {
                        answer = convertToScientific((Fraction)answerPiece.Value);
                    }
                    else if (answerPiece.Type.Equals("const"))
                    {
                        answer = convertToScientific(answerPiece.ConstValue);
                    }
                    else if (answerPiece.Type.Equals("vec"))
                    {
                        Vector v = (Vector)answerPiece.Value;
                        answer = $"<{convertToScientific(v.X)}, {convertToScientific(v.Y)}, {convertToScientific(v.Z)}>";
                    }
                }
                else if (showFractions)
                {
                    // Output in fraction notation
                    if (answerPiece.Type.Equals("num"))
                    {
                        answer = ((Fraction)answerPiece.Value).ToFracString();
                    }
                    else if (answerPiece.Type.Equals("const"))
                    {
                        answer = answerPiece.ConstValue.ToFracString();
                    }
                    else if (answerPiece.Type.Equals("vec"))
                    {
                        Vector v = (Vector)answerPiece.Value;
                        answer = $"<{v.X.ToFracString()}, {v.Y.ToFracString()}, {v.Z.ToFracString()}>";
                    }
                }
            }
            catch { }

            return answer;
        }
    }
}
