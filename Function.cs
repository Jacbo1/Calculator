using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Calculator
{
    internal class Function
    {
        public Dictionary<string, Piece> vars;
        public List<Formula> formulas = new List<Formula> ();
        private List<string> argVars;
        public string funcName = "NULL",
            originalString = "";

        public Function(string raw, Dictionary<string, Piece> vars, out int stop)
        {
            stop = -1;

            List<string> sections = new List<string>();
            int openCount = 0;
            string cumulative = "";
            char c;
            for (int i = 0; i < raw.Length; i++)
            {
                c = raw[i];
                if (c == '(')
                {
                    openCount++;
                    if (openCount == 1)
                    {
                        // Found function name
                        funcName = cumulative;
                        cumulative = "";
                        continue;
                    }
                }
                else if(c == '<')
                {
                    openCount++;
                }
                else if(c == '>' || c == ')')
                {
                    openCount--;
                    if (openCount == 0)
                    {
                        // Finished searching
                        stop = i + 1;
                        if (cumulative.Length > 0)
                        {
                            sections.Add(cumulative);
                        }
                        originalString = raw.Substring(0, stop);

                        switch (funcName)
                        {
                            default:
                                throw new FunctionException(funcName, "Unkown function.");
                            case "min":
                            case "max":
                                foreach (string section in sections)
                                {
                                    formulas.Add(new Formula(section, vars));
                                }
                                return;
                            case "clamp":
                                // num, min, max
                                if (sections.Count < 3)
                                {
                                    throw new FunctionException(funcName, "Not enough arguments.");
                                }
                                if (sections.Count > 3)
                                {
                                    throw new FunctionException(funcName, "Too many arguments.");
                                }
                                formulas.Add(new Formula(sections[0], vars));
                                formulas.Add(new Formula(sections[1], vars));
                                formulas.Add(new Formula(sections[2], vars));
                                return;
                            case "log":
                                // base?, num
                                if (sections.Count == 1)
                                {
                                    formulas.Add(new Formula(sections[0], vars));
                                }
                                else if (sections.Count == 2)
                                {
                                    formulas.Add(new Formula(sections[0], vars));
                                    formulas.Add(new Formula(sections[1], vars));
                                }
                                else if (sections.Count > 2)
                                {
                                    throw new FunctionException(funcName, "Too many arguments.");
                                }
                                else
                                {
                                    throw new FunctionException(funcName, "Not enough arguments.");
                                }
                                return;
                            case "round":
                                // num, digits?
                                if (sections.Count == 1)
                                {
                                    formulas.Add(new Formula(sections[0], vars));
                                }
                                else if (sections.Count == 2)
                                {
                                    formulas.Add(new Formula(sections[0], vars));
                                    formulas.Add(new Formula(sections[1], vars));
                                }
                                else
                                {
                                    if (sections.Count < 1)
                                    {
                                        throw new FunctionException(funcName, "Not enough arguments.");
                                    }
                                    if (sections.Count > 2)
                                    {
                                        throw new FunctionException(funcName, "Too many arguments.");
                                    }
                                }
                                return;
                            case "sum":
                            case "prod":
                                // Summation or product notation
                                // index of summation, lower bound, upper bound, formula
                                if (sections.Count < 4)
                                {
                                    throw new FunctionException(funcName, "Not enough arguments.");
                                }
                                if (sections.Count > 4)
                                {
                                    throw new FunctionException(funcName, "Too many arguments.");
                                }
                                argVars = new List<string> { sections[0] };
                                CloneVars(vars);
                                this.vars[sections[0]] = new Piece(1);
                                formulas.Add(new Formula(sections[1], this.vars));
                                formulas.Add(new Formula(sections[2], this.vars));
                                formulas.Add(new Formula(sections[3], this.vars));
                                return;
                            case "getx":
                            case "gety":
                            case "getz":
                            case "length":
                            case "norm":
                                formulas.Add(new Formula(sections[0], vars));
                                return;
                        }
                    }
                    else if (openCount < 0)
                    {
                        throw new FunctionException(funcName, "No opening parenthesis.");
                    }
                }
                if (c == ',' && openCount == 1)
                {
                    if (cumulative.Length > 0)
                    {
                        sections.Add(cumulative);
                    }
                    cumulative = "";
                }
                else
                {
                    cumulative += c;
                }
            }

            throw new FunctionException(funcName, "Unable to parse.");
        }

        private void CloneVars(Dictionary<string, Piece> vars)
        {
            if (this.vars is null)
            {
                this.vars = new Dictionary<string, Piece>();
            }
            foreach (KeyValuePair<string, Piece> x in vars)
            {
                this.vars[x.Key] = x.Value;
            }
        }

        private string CalcFormula(int index, out Piece result)
        {
            string answer = formulas[index].Calculate(out string work, true, false, true);
            work = work.Trim() + '\n';
            if (!Matching.RE_GenericNum.IsMatch(formulas[index].formula) && work.Length > 0)
            {
                work += Matching.Answer2Decimal(answer) + '\n';
            }
            result = new Piece(answer);
            if (result.Type == "error")
            {
                throw new FunctionException(funcName, answer);
            }
            return work;
        }

        public Piece Calculate(out string workOutput)
        {
            try
            {
                workOutput = "";
                switch (funcName)
                {
                    case "min":
                        // Min
                        {
                            if (formulas.Count == 0)
                            {
                                return new Piece(0);
                            }

                            // Initialize
                            workOutput += CalcFormula(0, out Piece min);

                            // Find min
                            for (int i = 1; i < formulas.Count; i++)
                            {
                                workOutput += CalcFormula(i, out Piece cur);

                                // Set min
                                bool isNum = cur.Type == "num";
                                bool isMinNum = min.Type == "num";
                                if (isNum && isMinNum)
                                {
                                    min = new Piece(Fraction.Min((Fraction)cur.Value, (Fraction)min.Value));
                                }
                                else if (isNum && !isMinNum)
                                {
                                    min = new Piece(Vector.Min((Fraction)cur.Value, (Vector)min.Value));
                                }
                                else if (!isNum && isMinNum)
                                {
                                    min = new Piece(Vector.Min((Vector)cur.Value, (Fraction)min.Value));
                                }
                                else
                                {
                                    min = new Piece(Vector.Min((Vector)cur.Value, (Vector)min.Value));
                                }
                            }
                            return min;
                        }

                    case "max":
                        // Max
                        {
                            if (formulas.Count == 0)
                            {
                                return new Piece(0);
                            }

                            // Initialize
                            workOutput += CalcFormula(0, out Piece max);

                            // Find max
                            for (int i = 1; i < formulas.Count; i++)
                            {
                                workOutput += CalcFormula(i, out Piece cur);

                                // Set max
                                bool isNum = cur.Type == "num";
                                bool isMaxNum = max.Type == "num";
                                if (isNum && isMaxNum)
                                {
                                    max = new Piece(Fraction.Max((Fraction)cur.Value, (Fraction)max.Value));
                                }
                                else if (isNum && !isMaxNum)
                                {
                                    max = new Piece(Vector.Max((Fraction)cur.Value, (Vector)max.Value));
                                }
                                else if (!isNum && isMaxNum)
                                {
                                    max = new Piece(Vector.Max((Vector)cur.Value, (Fraction)max.Value));
                                }
                                else
                                {
                                    max = new Piece(Vector.Max((Vector)cur.Value, (Vector)max.Value));
                                }
                            }
                            return max;
                        }

                    case "clamp":
                        // Clamp
                        {
                            workOutput += CalcFormula(0, out Piece num);
                            workOutput += CalcFormula(1, out Piece min);
                            workOutput += CalcFormula(2, out Piece max);

                            Piece result;
                            // Set max
                            bool isNum1 = num.Type == "num";
                            bool isNum2 = min.Type == "num";
                            if (isNum1 && isNum2)
                            {
                                result = new Piece(Fraction.Max((Fraction)num.Value, (Fraction)min.Value));
                            }
                            else if (isNum1 && !isNum2)
                            {
                                result = new Piece(Vector.Max((Fraction)num.Value, (Vector)min.Value));
                            }
                            else if (!isNum1 && isNum2)
                            {
                                result = new Piece(Vector.Max((Vector)num.Value, (Fraction)min.Value));
                            }
                            else
                            {
                                result = new Piece(Vector.Max((Vector)num.Value, (Vector)min.Value));
                            }

                            // Set min
                            isNum1 = result.Type == "num";
                            isNum2 = max.Type == "num";
                            if (isNum1 && isNum2)
                            {
                                return new Piece(Fraction.Max((Fraction)result.Value, (Fraction)max.Value));
                            }
                            else if (isNum1 && !isNum2)
                            {
                                return new Piece(Vector.Max((Fraction)result.Value, (Vector)max.Value));
                            }
                            else if (!isNum1 && isNum2)
                            {
                                return new Piece(Vector.Max((Vector)result.Value, (Fraction)max.Value));
                            }
                            return new Piece(Vector.Max((Vector)result.Value, (Vector)max.Value));
                        }

                    case "log":
                    // Log
                    // base?, num
                    {
                        if (formulas.Count == 1)
                        {
                            // Calculate log base 10
                            workOutput += CalcFormula(0, out Piece num);

                            if (num.Type == "num")
                            {
                                // Number
                                return new Piece(Math.Log10((double)(Fraction)num.Value));
                            }
                            else if (num.Type == "vec")
                            {
                                // Vector
                                Vector numVec = (Vector)num.Value;
                                return new Piece(new Vector(
                                    Math.Log10((double)numVec.X),
                                    Math.Log10((double)numVec.Y),
                                    Math.Log10((double)numVec.Z)));
                            }
                            else
                            {
                                throw new FunctionException($"Tried to take log log of type {num.Type}.");
                            }
                        }
                        // Calculate log with base
                        {
                            workOutput += CalcFormula(0, out Piece newBase);
                            workOutput += CalcFormula(1, out Piece num);

                            // Calculate log with base
                            bool isNum = num.Type == "num";
                            bool isBaseNum = newBase.Type == "num";

                            if (isNum && isBaseNum)
                            {
                                return new Piece(Math.Log(
                                    (double)(Fraction)num.Value,
                                    (double)(Fraction)newBase.Value));
                            }
                            else if (isNum && !isBaseNum)
                            {
                                Vector newBaseVec = (Vector)newBase.Value;
                                return new Piece(new Vector(
                                    Math.Log((double)(Fraction)num.Value, (double)newBaseVec.X),
                                    Math.Log((double)(Fraction)num.Value, (double)newBaseVec.Y),
                                    Math.Log((double)(Fraction)num.Value, (double)newBaseVec.Z)));
                            }
                            else if (!isNum && isBaseNum)
                            {
                                Vector numVec = (Vector)num.Value;
                                return new Piece(new Vector(
                                    Math.Log((double)numVec.X, (double)(Fraction)newBase.Value),
                                    Math.Log((double)numVec.Y, (double)(Fraction)newBase.Value),
                                    Math.Log((double)numVec.Z, (double)(Fraction)newBase.Value)));
                            }

                            {
                                Vector vec = (Vector)num.Value;
                                Vector newBaseVec = (Vector)newBase.Value;
                                return new Piece(new Vector(
                                    Math.Log((double)vec.X, (double)newBaseVec.X),
                                    Math.Log((double)vec.Y, (double)newBaseVec.Y),
                                    Math.Log((double)vec.Z, (double)newBaseVec.Z)));
                            }
                        }
                    }

                    case "round":
                        // Round
                        // num, digits?
                        {
                            workOutput += CalcFormula(0, out Piece num);

                            if (formulas.Count == 1)
                            {
                                // Round to integer
                                if (num.Type == "num")
                                {
                                    return new Piece(Fraction.Round((Fraction)num.Value));
                                }
                                return new Piece(Vector.Round((Vector)num.Value));
                            }

                            // Round to digits
                            workOutput += CalcFormula(1, out Piece digits);

                            bool isNum = num.Type == "num";
                            bool isDigitsNum = digits.Type == "num";
                            if (isNum && isDigitsNum)
                            {
                                return new Piece(Fraction.Round((Fraction)num.Value, (int)(Fraction)digits.Value));
                            }
                            else if (isNum && !isDigitsNum)
                            {
                                return new Piece(Vector.Round((Fraction)num.Value, (Vector)digits.Value));
                            }
                            else if (!isNum && isDigitsNum)
                            {
                                return new Piece(Vector.Round((Vector)num.Value, (int)(Fraction)digits.Value));
                            }
                            return new Piece(Vector.Round((Vector)num.Value, (Vector)digits.Value));
                        }

                    case "sum":
                        // Summation
                        // index of summation, lower bound, upper bound, formula
                        {
                            workOutput += CalcFormula(0, out Piece min);
                            if (min.Type == "const")
                            {
                                min = new Piece(min.ConstValue);
                            }
                            if (min.Type != "num")
                            {
                                throw new FunctionException(funcName, "Lower bound must be a number.");
                            }

                            workOutput += CalcFormula(1, out Piece max);
                            if (max.Type == "const")
                            {
                                max = new Piece(max.ConstValue);
                            }
                            if (max.Type != "num")
                            {
                                throw new FunctionException(funcName, "Upper bound must be a number.");
                            }

                            List<string> substrings = new List<string>();
                            List<Formula> subFormulas = new List<Formula>();

                            // Find sub formulas
                            {
                                string formula = formulas[2].formula;
                                int start = formula.IndexOf("_");
                                int stop = 0;
                                while (start != -1)
                                {
                                    substrings.Add(formula.Substring(stop, start - stop));
                                    stop = FindBreakoff(formula, start + 1);
                                    subFormulas.Add(new Formula(formula.Substring(start + 1, stop - start - 2), vars));
                                    start = formula.IndexOf("_", start + 1);
                                }
                                if (stop < formula.Length)
                                {
                                    substrings.Add(formula.Substring(stop));
                                }
                                else
                                {
                                    substrings.Add("");
                                }
                            }

                            string indexVarName = argVars[0];
                            int lowerBound = (int)(Fraction)min.Value;
                            int upperBound = (int)(Fraction)max.Value;
                            Piece[] results = new Piece[upperBound - lowerBound + 1];

                            // Calculate summation
                            for (int index = lowerBound; index <= upperBound; index++)
                            {
                                Piece newIndexPiece = new Piece(index);
                                formulas[2].SetVar(indexVarName, newIndexPiece);

                                // Replace sub formulas
                                string formula = substrings[0];
                                for (int i = 0; i < subFormulas.Count; i++)
                                {
                                    subFormulas[i].SetVar(indexVarName, newIndexPiece);
                                    string subanswer = subFormulas[i].Calculate();
                                    if (Fraction.TryParse(subanswer, out Fraction subindex))
                                    {
                                        formula += (int)subindex + substrings[i + 1];
                                    }
                                    else
                                    {
                                        throw new FunctionException(funcName, subanswer);
                                    }
                                }
                                formulas[2].formula = formula.Replace("_" + indexVarName, index.ToString());

                                // Calculate
                                string answer = formulas[2].Calculate();
                                Piece piece = new Piece(answer);
                                if (piece.Type == "error")
                                {
                                    throw new FunctionException(funcName, answer);
                                }
                                results[index - lowerBound] = piece;

                                // Update variables
                                string aName = "a" + index;
                                formulas[2].SetVar(aName, piece);
                                foreach (Formula subFormula in subFormulas)
                                {
                                    subFormula.SetVar(aName, piece);
                                }
                            }

                            Piece sum = new Piece(0);
                            foreach (Piece piece in results)
                            {
                                bool isNum1 = sum.Type == "num";
                                bool isNum2 = piece.Type == "num";
                                if (isNum1 && isNum2)
                                {
                                    sum = new Piece((Fraction)sum.Value + (Fraction)piece.Value);
                                }
                                else if (isNum1 && !isNum2)
                                {
                                    sum = new Piece((Fraction)sum.Value + (Vector)piece.Value);
                                }
                                else if (!isNum1 && isNum2)
                                {
                                    sum = new Piece((Vector)sum.Value + (Fraction)piece.Value);
                                }
                                else
                                {
                                    sum = new Piece((Vector)sum.Value + (Vector)piece.Value);
                                }
                            }

                            return sum;
                        }

                    case "prod":
                        // Product notation
                        // var, lower bound, upper bound, formula
                        {
                            workOutput += CalcFormula(0, out Piece min);
                            if (min.Type == "const")
                            {
                                min = new Piece(min.ConstValue);
                            }
                            if (min.Type != "num")
                            {
                                throw new FunctionException(funcName, "Lower bound must be a number.");
                            }

                            workOutput += CalcFormula(1, out Piece max);
                            if (max.Type == "const")
                            {
                                max = new Piece(max.ConstValue);
                            }
                            if (max.Type != "num")
                            {
                                throw new FunctionException(funcName, "Upper bound must be a number.");
                            }

                            List<string> substrings = new List<string>();
                            List<Formula> subFormulas = new List<Formula>();

                            // Find sub formulas
                            {
                                string formula = formulas[2].formula;
                                int start = formula.IndexOf("_");
                                int stop = 0;
                                while (start != -1)
                                {
                                    substrings.Add(formula.Substring(stop, start - stop));
                                    stop = FindBreakoff(formula, start + 1);
                                    subFormulas.Add(new Formula(formula.Substring(start + 1, stop - start - 2), vars));
                                    start = formula.IndexOf("_", start + 1);
                                }
                                if (stop < formula.Length)
                                {
                                    substrings.Add(formula.Substring(stop));
                                }
                                else
                                {
                                    substrings.Add("");
                                }
                            }

                            string indexVarName = argVars[0];
                            int lowerBound = (int)(Fraction)min.Value;
                            int upperBound = (int)(Fraction)max.Value;
                            Piece[] results = new Piece[upperBound - lowerBound + 1];

                            // Calculate product
                            for (int index = lowerBound; index <= upperBound; index++)
                            {
                                Piece newIndexPiece = new Piece(index);
                                formulas[2].SetVar(indexVarName, newIndexPiece);

                                // Replace sub formulas
                                string formula = substrings[0];
                                for (int i = 0; i < subFormulas.Count; i++)
                                {
                                    subFormulas[i].SetVar(indexVarName, newIndexPiece);
                                    string subanswer = subFormulas[i].Calculate();
                                    if (Fraction.TryParse(subanswer, out Fraction subindex))
                                    {
                                        formula += (int)subindex + substrings[i + 1];
                                    }
                                    else
                                    {
                                        throw new FunctionException(funcName, subanswer);
                                    }
                                }
                                formulas[2].formula = formula.Replace("_" + indexVarName, index.ToString());

                                // Calculate
                                string answer = formulas[2].Calculate();
                                Piece piece = new Piece(answer);
                                if (piece.Type == "error")
                                {
                                    throw new FunctionException(funcName, answer);
                                }
                                results[index - lowerBound] = piece;

                                // Update variables
                                string aName = "a" + index;
                                formulas[2].SetVar(aName, piece);
                                foreach (Formula subFormula in subFormulas)
                                {
                                    subFormula.SetVar(aName, piece);
                                }
                            }

                            if (results.Length == 0)
                            {
                                return new Piece(0);
                            }

                            Piece product = new Piece(1);
                            foreach (Piece piece in results)
                            {
                                bool isNum1 = product.Type == "num";
                                bool isNum2 = piece.Type == "num";
                                if (isNum1 && isNum2)
                                {
                                    product = new Piece((Fraction)product.Value * (Fraction)piece.Value);
                                }
                                else if (isNum1 && !isNum2)
                                {
                                    product = new Piece((Fraction)product.Value * (Vector)piece.Value);
                                }
                                else if (!isNum1 && isNum2)
                                {
                                    product = new Piece((Vector)product.Value * (Fraction)piece.Value);
                                }
                                else
                                {
                                    product = new Piece((Vector)product.Value * (Vector)piece.Value);
                                }
                            }

                            return product;
                        }

                    case "getx":
                        {
                            workOutput += CalcFormula(0, out Piece piece);
                            if (piece.Type == "vec")
                            {
                                return new Piece(((Vector)piece.Value).X);
                            }
                            throw new FunctionException(funcName, "Attempted to get x component of non-vector.");
                        }

                    case "gety":
                        {
                            workOutput += CalcFormula(0, out Piece piece);
                            if (piece.Type == "vec")
                            {
                                return new Piece(((Vector)piece.Value).Y);
                            }
                            throw new FunctionException(funcName, "Attempted to get y component of non-vector.");
                        }

                    case "getz":
                        {
                            workOutput += CalcFormula(0, out Piece piece);
                            if (piece.Type == "vec")
                            {
                                return new Piece(((Vector)piece.Value).Z);
                            }
                            throw new FunctionException(funcName, "Attempted to get z component of non-vector.");
                        }

                    case "length":
                        {
                            workOutput += CalcFormula(0, out Piece piece);
                            if (piece.Type == "vec")
                            {
                                Vector vec = (Vector)piece.Value;
                                return new Piece(Fraction.Pow(vec.X * vec.X + vec.Y * vec.Y + vec.Z * vec.Z, 0.5));
                            }
                            throw new FunctionException(funcName, "Attempted to get length of non-vector.");
                        }

                    case "norm":
                        {
                            workOutput += CalcFormula(0, out Piece piece);
                            if (piece.Type == "vec")
                            {
                                Vector vec = (Vector)piece.Value;
                                Fraction length = Fraction.Pow(vec.X * vec.X + vec.Y * vec.Y + vec.Z * vec.Z, 0.5);
                                if (length == 0)
                                {
                                    return new Piece(new Vector());
                                }
                                return new Piece(vec / length);
                            }
                            throw new FunctionException(funcName, "Attempted to normalize non-vector.");
                        }
                }
                return null;
            }
            catch (FractionDoubleParsingException e)
            {
                throw new FunctionException(funcName, e.Message);
            }
        }

        private static int FindBreakoff(string input, int index)
        {
            int openCount = 0;
            foreach (Match match in Matching.RE_DefaultPieces.Matches(input, index))
            {
                if (match.Value == "(")
                {
                    openCount++;
                    continue;
                }
                if (match.Value == ")")
                {
                    openCount--;
                    continue;
                }
                if (openCount <= 0 && Matching.IsOperator(match.Value))
                {
                    return match.Index;
                }
            }
            return input.Length + 1;
        }

        public override string ToString()
        {
            switch (funcName)
            {
                case "min":
                    {
                        string s = "min(";
                        bool first = true;
                        foreach (Formula formula in formulas)
                        {
                            if (first)
                            {
                                first = false;
                                s += formula.formula;
                            }
                            else
                            {
                                s += ", " + formula.formula;
                            }
                        }
                        return s + ')';
                    }
                case "max":
                    {
                        string s = "max(";
                        bool first = true;
                        foreach (Formula formula in formulas)
                        {
                            if (first)
                            {
                                first = false;
                                s += formula.formula;
                            }
                            else
                            {
                                s += ", " + formula.formula;
                            }
                        }
                        return s + ')';
                    }
                case "log":
                    if (formulas.Count == 1)
                    {
                        return $"log({formulas[0].formula})";
                    }
                    return $"log({formulas[1].formula}, base = {formulas[0].formula})";
                case "round":
                    if (formulas.Count == 1)
                    {
                        return $"round({formulas[0].formula})";
                    }
                    return $"round({formulas[0].formula}, digits = {formulas[1].formula})";
                case "sum":
                    return $"∑({argVars[0]}={formulas[0].formula} to {formulas[1].formula}, a({argVars[0]})={formulas[2].formula})";
                case "prod":
                    return $"∏({argVars[0]}={formulas[0].formula} to {formulas[1].formula}, a({argVars[0]})={formulas[2].formula})";
            }
            return originalString;
        }
    }
}
