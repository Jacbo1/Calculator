﻿using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Calculator
{
    internal class FormulaGroup
    {
        private static Regex varRegex = new Regex(@"(\S+)=(\S+)", RegexOptions.Compiled);
        private static Regex whitespace = new Regex(@"\s", RegexOptions.Compiled);

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
                    string work;
                    string varName = match.Groups[1].Value;
                    answer = new Formula(match.Groups[2].Value, vars).Calculate(out work);
                    work += $"\n{answer}";
                    Piece varPiece = new Piece(answer);
                    varPiece.IsVar = true;
                    varPiece.VarName = varName;
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
                    string work;
                    answer = new Formula(line, vars).Calculate(out work, false, true);
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
            return answer;
        }
    }
}
