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
                    answer = Matching.Answer2Decimal(new Formula(right, vars).Calculate(out string work, false, false, false));
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
                    answer = Matching.Answer2Decimal(new Formula(line, vars).Calculate(out string work, false, true, false));
                    work = work.Trim();
                    workOutput += $"{work}\n{answer}\n\n";
                }
            }

            return answer;
        }
    }
}
