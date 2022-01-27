using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Calculator
{
    internal class FormulaGroup
    {
        private static readonly Regex varRegex = new Regex(@"(\S+)=(\S+)", RegexOptions.Compiled);
        private static readonly Regex whitespace = new Regex(@"\s", RegexOptions.Compiled);

        public static string Calculate(string input, out string workOutput)
        {
            workOutput = "";
            string answer = "";

            List<Formula> formulas = new List<Formula>();
            Dictionary<string, Piece> vars = new Dictionary<string, Piece>();
            bool first = true;
            foreach (string line2 in input.Split('\n'))
            {
                string line = whitespace.Replace(line2, "");
                Match match = varRegex.Match(line);
                if (match.Success)
                {
                    // This line is a variable assignment
                    string varName = match.Groups[1].Value;
                    answer = new Formula(match.Groups[2].Value, vars).Calculate(out string work);
                    work += $"\n{answer}";
                    Piece varPiece = new Piece(answer)
                    {
                        IsVar = true,
                        VarName = varName
                    };
                    vars[varName] = varPiece;

                    foreach (Formula formula in formulas)
                    {
                        formula.SetVar(varName, varPiece);
                    }
                    if (first)
                    {
                        first = false;
                        workOutput += work;
                    }
                    else
                    {
                        workOutput += $"\n\n{work}";
                    }
                }
                else if (line.Length > 0)
                {
                    answer = new Formula(line, vars).Calculate(out string work, false, true);
                    work += $"\n{answer}";
                    if (first)
                    {
                        first = false;
                        workOutput += work;
                    }
                    else
                    {
                        workOutput += $"\n\n{work}";
                    }
                }
            }

            if (Regexes.RE_Fraction.IsMatch(answer) && Fraction.TryParse(answer, out Fraction frac))
            {
                return frac.ToString();
            }

            {
                Match match = Regexes.RE_VectorFraction.Match(answer);
                if (match.Success)
                {
                    Fraction x, y, z;
                    if (!Fraction.TryParse(match.Groups[1].Value, out x)) { return answer; }
                    if (!Fraction.TryParse(match.Groups[10].Value, out y)) { return answer; }
                    if (!Fraction.TryParse(match.Groups[19].Value, out z)) { return answer; }
                    return $"<{x}, {y}, {z}>";
                }
            }

            return answer;
        }
    }
}
