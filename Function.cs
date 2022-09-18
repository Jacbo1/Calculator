using System;
using System.Collections.Generic;
using System.Numerics;

namespace Calculator
{
    internal struct Function
    {
        public Dictionary<string, Piece> Vars;
        public List<Formula> Formulas;
        public string FuncName, OriginalString;
        private List<string> argVars;

        public Function(string raw, Dictionary<string, Piece> vars, out int stop)
        {
            Formulas = new List<Formula>();
            FuncName = "NULL";
            OriginalString = "";
            Vars = null;
            argVars = null;

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
                        FuncName = cumulative;
                        cumulative = "";
                        continue;
                    }
                }
                else if (c == '<')
                    openCount++;
                else if (c == '>' || c == ')')
                {
                    openCount--;
                    if (openCount == 0)
                    {
                        // Finished searching
                        stop = i + 1;
                        if (cumulative.Length > 0)
                            sections.Add(cumulative);

                        OriginalString = raw.Substring(0, stop);

                        switch (FuncName)
                        {
                            default:
                                throw new FunctionException(FuncName, "Unkown function.");
                            case "min":
                            case "max":
                            case "band":
                            case "bor":
                            case "bxor":
                                foreach (string section in sections)
                                    Formulas.Add(new Formula(section, vars));
                                return;
                            case "clamp":
                                // num, min, max
                                if (sections.Count < 3)
                                    throw new FunctionException(FuncName, "Not enough arguments.");
                                if (sections.Count > 3)
                                    throw new FunctionException(FuncName, "Too many arguments.");
                                Formulas.Add(new Formula(sections[0], vars));
                                Formulas.Add(new Formula(sections[1], vars));
                                Formulas.Add(new Formula(sections[2], vars));
                                return;
                            case "log":
                                // base?, num
                                if (sections.Count < 1)
                                    throw new FunctionException(FuncName, "Not enough arguments.");
                                if (sections.Count > 2)
                                    throw new FunctionException(FuncName, "Too many arguments.");
                                if (sections.Count == 1)
                                    Formulas.Add(new Formula(sections[0], vars));
                                else // 2
                                {
                                    Formulas.Add(new Formula(sections[0], vars));
                                    Formulas.Add(new Formula(sections[1], vars));
                                }
                                return;
                            case "round":
                            case "bnot":
                                // num, digits?
                                if (sections.Count < 1)
                                    throw new FunctionException(FuncName, "Not enough arguments.");
                                if (sections.Count > 2)
                                    throw new FunctionException(FuncName, "Too many arguments.");
                                if (sections.Count == 1)
                                    Formulas.Add(new Formula(sections[0], vars));
                                else // 2
                                {
                                    Formulas.Add(new Formula(sections[0], vars));
                                    Formulas.Add(new Formula(sections[1], vars));
                                }
                                return;
                            case "sum":
                            case "prod":
                                // Summation or product notation
                                // index of summation, lower bound, upper bound, formula
                                if (sections.Count < 4)
                                    throw new FunctionException(FuncName, "Not enough arguments.");
                                if (sections.Count > 4)
                                    throw new FunctionException(FuncName, "Too many arguments.");

                                argVars = new List<string> { sections[0] };
                                CloneVars(vars);
                                Vars[sections[0]] = new Piece(1);
                                Formulas.Add(new Formula(sections[1], Vars));
                                Formulas.Add(new Formula(sections[2], Vars));
                                Formulas.Add(new Formula(sections[3], Vars));
                                return;
                            case "getx":
                            case "gety":
                            case "getz":
                            case "length":
                            case "norm":
                            case "ln":
                                if (sections.Count < 1)
                                    throw new FunctionException(FuncName, "Not enough arguments.");
                                if (sections.Count > 1)
                                    throw new FunctionException(FuncName, "Too many arguments.");

                                Formulas.Add(new Formula(sections[0], vars));
                                return;
                            case "atan2":
                            case "bshift":
                                if (sections.Count < 2)
                                    throw new FunctionException(FuncName, "Not enough arguments.");
                                if (sections.Count > 2)
                                    throw new FunctionException(FuncName, "Too many arguments.");

                                Formulas.Add(new Formula(sections[0], vars));
                                Formulas.Add(new Formula(sections[1], vars));
                                return;
                        }
                    }
                    else if (openCount < 0)
                    {
                        throw new FunctionException(FuncName, "No opening parenthesis.");
                    }
                }
                if (c == ',' && openCount == 1)
                {
                    if (cumulative.Length > 0)
                        sections.Add(cumulative);

                    cumulative = "";
                }
                else
                {
                    cumulative += c;
                }
            }

            throw new FunctionException(FuncName, "Unable to parse.");
        }

        private void CloneVars(Dictionary<string, Piece> vars)
        {
            if (Vars is null)
                Vars = new Dictionary<string, Piece>();

            foreach (KeyValuePair<string, Piece> x in vars)
                Vars[x.Key] = x.Value;
        }

        private string CalcFormula(int index, out Piece result)
        {
            string answer = Formulas[index].Calculate(out string work, true, false, true);
            work = work.Trim() + '\n';
            if (!Matching.RE_GenericNum.IsMatch(Formulas[index].formula) && work.Length > 0)
                work += Matching.Answer2Decimal(answer) + '\n';

            result = new Piece(answer);
            if (result.Type == "error")
                throw new FunctionException(FuncName, answer);

            return work;
        }

        public Piece Calculate(out string workOutput)
        {
            try
            {
                workOutput = "";
                switch (FuncName)
                {
                    case "min":
                        // Min
                        {
                            if (Formulas.Count == 0)
                                return new Piece(0);

                            // Initialize
                            workOutput += CalcFormula(0, out Piece min);

                            // Find min
                            for (int i = 1; i < Formulas.Count; i++)
                            {
                                workOutput += CalcFormula(i, out Piece cur);

                                // Set min
                                bool isNum = cur.Type == "num";
                                bool isMinNum = min.Type == "num";
                                if (isNum && isMinNum)
                                    min = new Piece(Fraction.Min((Fraction)cur.Value, (Fraction)min.Value));
                                else if (isNum && !isMinNum)
                                    min = new Piece(Utils.Op((Fraction)cur.Value, (Vector)min.Value, Fraction.Min));
                                else if (!isNum && isMinNum)
                                    min = new Piece(Utils.Op((Vector)cur.Value, (Fraction)min.Value, Fraction.Min));
                                else
                                    min = new Piece(Utils.Op((Vector)cur.Value, (Vector)min.Value, (Func<Fraction, Fraction, Fraction>)Fraction.Min));
                            }
                            return min;
                        }

                    case "max":
                        // Max
                        {
                            if (Formulas.Count == 0)
                                return new Piece(0);

                            // Initialize
                            workOutput += CalcFormula(0, out Piece max);

                            // Find max
                            for (int i = 1; i < Formulas.Count; i++)
                            {
                                workOutput += CalcFormula(i, out Piece cur);

                                // Set max
                                bool isNum = cur.Type == "num";
                                bool isMaxNum = max.Type == "num";
                                if (isNum && isMaxNum)
                                    max = new Piece(Fraction.Max((Fraction)cur.Value, (Fraction)max.Value));
                                else if (isNum && !isMaxNum)
                                    max = new Piece(Utils.Op((Fraction)cur.Value, (Vector)max.Value, Fraction.Max));
                                else if (!isNum && isMaxNum)
                                    max = new Piece(Utils.Op((Vector)cur.Value, (Fraction)max.Value, Fraction.Max));
                                else
                                    max = new Piece(Utils.Op((Vector)cur.Value, (Vector)max.Value, (Func<Fraction, Fraction, Fraction>)Fraction.Max));
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
                                result = new Piece(Fraction.Max((Fraction)num.Value, (Fraction)min.Value));
                            else if (isNum1 && !isNum2)
                                result = new Piece(Utils.Op((Fraction)num.Value, (Vector)min.Value, Fraction.Max));
                            else if (!isNum1 && isNum2)
                                result = new Piece(Utils.Op((Vector)num.Value, (Fraction)min.Value, Fraction.Max));
                            else
                                result = new Piece(Utils.Op((Vector)num.Value, (Vector)min.Value, (Func<Fraction, Fraction, Fraction>)Fraction.Max));

                            // Set min
                            isNum1 = result.Type == "num";
                            isNum2 = max.Type == "num";
                            if (isNum1 && isNum2)
                                return new Piece(Fraction.Max((Fraction)result.Value, (Fraction)max.Value));
                            if (isNum1 && !isNum2)
                                return new Piece(Utils.Op((Fraction)result.Value, (Vector)max.Value, Fraction.Max));
                            if (!isNum1 && isNum2)
                                return new Piece(Utils.Op((Vector)result.Value, (Fraction)max.Value, Fraction.Max));
                            return new Piece(Utils.Op((Vector)result.Value, (Vector)max.Value, (Func<Fraction, Fraction, Fraction>)Fraction.Max));
                        }

                    case "log":
                        // Log
                        // base?, num
                        {
                            if (Formulas.Count == 1)
                            {
                                // Calculate log base 10
                                workOutput += CalcFormula(0, out Piece num);

                                if (num.Type == "num")
                                    // Number
                                    return new Piece(Math.Log10((double)(Fraction)num.Value));
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
                                    return new Piece(Math.Log(
                                        (double)(Fraction)num.Value,
                                        (double)(Fraction)newBase.Value));
                                if (isNum && !isBaseNum)
                                {
                                    Vector newBaseVec = (Vector)newBase.Value;
                                    return new Piece(new Vector(
                                        Math.Log((double)(Fraction)num.Value, (double)newBaseVec.X),
                                        Math.Log((double)(Fraction)num.Value, (double)newBaseVec.Y),
                                        Math.Log((double)(Fraction)num.Value, (double)newBaseVec.Z)));
                                }
                                if (!isNum && isBaseNum)
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

                            if (Formulas.Count == 1)
                                // Round to integer
                                return num.Type == "num" ?
                                    new Piece(Fraction.Round((Fraction)num.Value)) :
                                    new Piece(Utils.Op((Vector)num.Value, Fraction.Round));

                            // Round to digits
                            workOutput += CalcFormula(1, out Piece digits);

                            bool isNum = num.Type == "num";
                            bool isDigitsNum = digits.Type == "num";
                            if (isNum && isDigitsNum)
                                return new Piece(Fraction.Round((Fraction)num.Value, (int)(Fraction)digits.Value));
                            if (isNum && !isDigitsNum)
                                return new Piece(Utils.Op((Fraction)num.Value, (Vector)digits.Value, Fraction.Round));
                            if (!isNum && isDigitsNum)
                                return new Piece(Utils.Op((Vector)num.Value, (int)(Fraction)digits.Value, Fraction.Round));
                            return new Piece(Utils.Op((Vector)num.Value, (Vector)digits.Value, Fraction.Round));
                        }

                    case "sum":
                        // Summation
                        // index of summation, lower bound, upper bound, formula
                        {
                            workOutput += CalcFormula(0, out Piece min);
                            if (min.Type == "const")
                                min = new Piece(min.ConstValue);
                            else if (min.Type != "num")
                                throw new FunctionException(FuncName, "Lower bound must be a number.");

                            workOutput += CalcFormula(1, out Piece max);
                            if (max.Type == "const")
                                max = new Piece(max.ConstValue);
                            else if (max.Type != "num")
                                throw new FunctionException(FuncName, "Upper bound must be a number.");

                            List<string> substrings = new List<string>();
                            List<Formula> subFormulas = new List<Formula>();

                            // Find sub formulas
                            {
                                bool xIsVar = argVars[0] == "x" || Formulas[2].vars.ContainsKey("x");
                                string formula = Formulas[2].formula;
                                int start = formula.IndexOf("_");
                                int stop = 0;
                                while (start != -1)
                                {
                                    substrings.Add(formula.Substring(stop, start - stop));
                                    stop = FindBreakoff(formula, start + 1, xIsVar);
                                    subFormulas.Add(new Formula(formula.Substring(start + 1, stop - start), Vars));
                                    start = formula.IndexOf("_", start + 1);
                                    stop++;
                                }

                                if (stop < formula.Length)
                                    substrings.Add(formula.Substring(stop));
                                else
                                    substrings.Add("");
                            }

                            string indexVarName = argVars[0];
                            int lowerBound = (int)(Fraction)min.Value;
                            int upperBound = (int)(Fraction)max.Value;
                            Piece[] results = new Piece[upperBound - lowerBound + 1];

                            // Calculate summation
                            for (int index = lowerBound; index <= upperBound; index++)
                            {
                                Piece newIndexPiece = new Piece(index);
                                Formulas[2].SetVar(indexVarName, newIndexPiece);

                                // Replace sub formulas
                                Formulas[2].formula = substrings[0];
                                for (int i = 0; i < subFormulas.Count; i++)
                                {
                                    subFormulas[i].SetVar(indexVarName, newIndexPiece);
                                    string subanswer = subFormulas[i].Calculate();

                                    if (Fraction.TryParse(subanswer, out Fraction? subindex))
                                        Formulas[2].formula += (int)subindex + substrings[i + 1];
                                    else
                                        throw new FunctionException(FuncName, subanswer);
                                }

                                // Calculate
                                string answer = Formulas[2].Calculate();
                                Piece piece = new Piece(answer);
                                if (piece.Type == "error")
                                    throw new FunctionException(FuncName, answer);

                                results[index - lowerBound] = piece;

                                // Update variables
                                string aName = "a" + index;
                                Formulas[2].SetVar(aName, piece);
                                foreach (Formula subFormula in subFormulas)
                                    subFormula.SetVar(aName, piece);
                            }

                            Piece sum = new Piece(0);
                            foreach (Piece piece in results)
                            {
                                bool isNum1 = sum.Type == "num";
                                bool isNum2 = piece.Type == "num";
                                if (isNum1 && isNum2)
                                    sum = new Piece((Fraction)sum.Value + (Fraction)piece.Value);
                                else if (isNum1 && !isNum2)
                                    sum = new Piece((Fraction)sum.Value + (Vector)piece.Value);
                                else if (!isNum1 && isNum2)
                                    sum = new Piece((Vector)sum.Value + (Fraction)piece.Value);
                                else
                                    sum = new Piece((Vector)sum.Value + (Vector)piece.Value);
                            }

                            return sum;
                        }

                    case "prod":
                        // Product notation
                        // var, lower bound, upper bound, formula
                        {
                            workOutput += CalcFormula(0, out Piece min);
                            if (min.Type == "const")
                                min = new Piece(min.ConstValue);
                            else if (min.Type != "num")
                                throw new FunctionException(FuncName, "Lower bound must be a number.");

                            workOutput += CalcFormula(1, out Piece max);
                            if (max.Type == "const")
                                max = new Piece(max.ConstValue);
                            else if (max.Type != "num")
                                throw new FunctionException(FuncName, "Upper bound must be a number.");

                            List<string> substrings = new List<string>();
                            List<Formula> subFormulas = new List<Formula>();

                            // Find sub formulas
                            {
                                bool xIsVar = argVars[0] == "x" || Formulas[2].vars.ContainsKey("x");
                                string formula = Formulas[2].formula;
                                int start = formula.IndexOf("_");
                                int stop = 0;
                                while (start != -1)
                                {
                                    substrings.Add(formula.Substring(stop, start - stop));
                                    stop = FindBreakoff(formula, start + 1, xIsVar);
                                    subFormulas.Add(new Formula(formula.Substring(start + 1, stop - start), Vars));
                                    start = formula.IndexOf("_", start + 1);
                                    stop++;
                                }

                                if (stop < formula.Length)
                                    substrings.Add(formula.Substring(stop));
                                else
                                    substrings.Add("");
                            }

                            string indexVarName = argVars[0];
                            int lowerBound = (int)(Fraction)min.Value;
                            int upperBound = (int)(Fraction)max.Value;
                            Piece[] results = new Piece[upperBound - lowerBound + 1];

                            // Calculate product
                            for (int index = lowerBound; index <= upperBound; index++)
                            {
                                Piece newIndexPiece = new Piece(index);
                                Formulas[2].SetVar(indexVarName, newIndexPiece);

                                // Replace sub formulas
                                Formulas[2].formula = substrings[0];
                                for (int i = 0; i < subFormulas.Count; i++)
                                {
                                    subFormulas[i].SetVar(indexVarName, newIndexPiece);
                                    string subanswer = subFormulas[i].Calculate();
                                    if (Fraction.TryParse(subanswer, out Fraction? subindex))
                                        Formulas[2].formula += (int)subindex + substrings[i + 1];
                                    else
                                        throw new FunctionException(FuncName, subanswer);
                                }

                                // Calculate
                                string answer = Formulas[2].Calculate();
                                Piece piece = new Piece(answer);
                                if (piece.Type == "error")
                                    throw new FunctionException(FuncName, answer);

                                results[index - lowerBound] = piece;

                                // Update variables
                                string aName = "a" + index;
                                Formulas[2].SetVar(aName, piece);
                                foreach (Formula subFormula in subFormulas)
                                    subFormula.SetVar(aName, piece);
                            }

                            if (results.Length == 0)
                                return new Piece(0);

                            Piece product = new Piece(1);
                            foreach (Piece piece in results)
                            {
                                bool isNum1 = product.Type == "num";
                                bool isNum2 = piece.Type == "num";
                                if (isNum1 && isNum2)
                                    product = new Piece((Fraction)product.Value * (Fraction)piece.Value);
                                else if (isNum1 && !isNum2)
                                    product = new Piece((Fraction)product.Value * (Vector)piece.Value);
                                else if (!isNum1 && isNum2)
                                    product = new Piece((Vector)product.Value * (Fraction)piece.Value);
                                else
                                    product = new Piece((Vector)product.Value * (Vector)piece.Value);
                            }

                            return product;
                        }

                    case "getx":
                        {
                            workOutput += CalcFormula(0, out Piece piece);
                            if (piece.Type == "vec")
                                return new Piece(((Vector)piece.Value).X);

                            throw new FunctionException(FuncName, "Attempted to get x component of non-vector.");
                        }

                    case "gety":
                        {
                            workOutput += CalcFormula(0, out Piece piece);
                            if (piece.Type == "vec")
                                return new Piece(((Vector)piece.Value).Y);

                            throw new FunctionException(FuncName, "Attempted to get y component of non-vector.");
                        }

                    case "getz":
                        {
                            workOutput += CalcFormula(0, out Piece piece);
                            if (piece.Type == "vec")
                                return new Piece(((Vector)piece.Value).Z);

                            throw new FunctionException(FuncName, "Attempted to get z component of non-vector.");
                        }

                    case "length":
                        {
                            workOutput += CalcFormula(0, out Piece piece);
                            if (piece.Type == "vec")
                            {
                                Vector vec = (Vector)piece.Value;
                                return new Piece(Fraction.Pow(vec.X * vec.X + vec.Y * vec.Y + vec.Z * vec.Z, Fraction.Half));
                            }
                            throw new FunctionException(FuncName, "Attempted to get length of non-vector.");
                        }

                    case "norm":
                        {
                            workOutput += CalcFormula(0, out Piece piece);
                            if (piece.Type == "vec")
                            {
                                Vector vec = (Vector)piece.Value;
                                Fraction length = Fraction.Pow(vec.X * vec.X + vec.Y * vec.Y + vec.Z * vec.Z, Fraction.Half);
                                return new Piece(length == 0 ? new Vector(0, 0, 0) : vec / length);
                            }
                            throw new FunctionException(FuncName, "Attempted to normalize non-vector.");
                        }

                    case "atan2":
                        {
                            workOutput += CalcFormula(0, out Piece y);
                            workOutput += CalcFormula(1, out Piece x);

                            bool isNum1 = y.Type == "num";
                            bool isNum2 = x.Type == "num";

                            if (isNum1 && isNum2)
                                return new Piece(Fraction.Atan2((Fraction)y.Value, (Fraction)x.Value));
                            if (isNum1 && !isNum2)
                                return new Piece(Utils.Op((Fraction)y.Value, (Vector)x.Value, Fraction.Atan2));
                            if (!isNum1 && isNum2)
                                return new Piece(Utils.Op((Vector)y.Value, (Fraction)x.Value, Fraction.Atan2));
                            return new Piece(Utils.Op((Vector)y.Value, (Vector)x.Value, (Func<Fraction, Fraction, Fraction>)Fraction.Atan2));
                        }

                    case "ln":
                        {
                            workOutput += CalcFormula(0, out Piece n);

                            if (n.Type == "num")
                                return new Piece(Math.Log((double)(Fraction)n.Value));
                            else
                            {
                                Vector v = (Vector)n.Value;
                                return new Piece(new Vector(
                                    Math.Log((double)v.X),
                                    Math.Log((double)v.Y),
                                    Math.Log((double)v.Z)));
                            }
                        }
                    case "band":
                        {
                            // Bitwise AND
                            if (Formulas.Count == 0)
                                return new Piece(0);

                            // Initialize
                            workOutput += CalcFormula(0, out Piece result);

                            for (int i = 1; i < Formulas.Count; i++)
                            {
                                workOutput += CalcFormula(i, out Piece cur);

                                bool isNum1 = cur.Type == "num";
                                bool isNum2 = result.Type == "num";

                                if (isNum1 && isNum2)
                                {
                                    result = new Piece((BigInteger)(Fraction)cur.Value & (BigInteger)(Fraction)result.Value);
                                    continue;
                                }

                                if (isNum1 && !isNum2)
                                {
                                    BigInteger cur1 = (BigInteger)(Fraction)cur.Value;
                                    Vector result1 = (Vector)result.Value;
                                    result = new Piece(new Vector(cur1 & (BigInteger)result1.X, cur1 & (BigInteger)result1.Y, cur1 & (BigInteger)result1.Z));
                                    continue;
                                }

                                if (!isNum1 && isNum2)
                                {
                                    Vector cur1 = (Vector)cur.Value;
                                    BigInteger result1 = (BigInteger)(Fraction)result.Value;
                                    result = new Piece(new Vector((BigInteger)cur1.X & result1, (BigInteger)cur1.Y & result1, (BigInteger)cur1.Z & result1));
                                    continue;
                                }

                                {
                                    Vector cur1 = (Vector)cur.Value;
                                    Vector result1 = (Vector)result.Value;
                                    result = new Piece(new Vector((BigInteger)cur1.X & (BigInteger)result1.X, (BigInteger)cur1.Y & (BigInteger)result1.Y, (BigInteger)cur1.Z & (BigInteger)result1.Z));
                                }
                            }

                            return result;
                        }
                    case "bor":
                        {
                            // Bitwise OR
                            if (Formulas.Count == 0)
                                return new Piece(0);

                            // Initialize
                            workOutput += CalcFormula(0, out Piece result);

                            for (int i = 1; i < Formulas.Count; i++)
                            {
                                workOutput += CalcFormula(i, out Piece cur);

                                bool isNum1 = cur.Type == "num";
                                bool isNum2 = result.Type == "num";

                                if (isNum1 && isNum2)
                                {
                                    result = new Piece((BigInteger)(Fraction)cur.Value | ((BigInteger)(Fraction)result.Value));
                                    continue;
                                }

                                if (isNum1 && !isNum2)
                                {
                                    BigInteger cur1 = (BigInteger)(Fraction)cur.Value;
                                    Vector result1 = (Vector)result.Value;
                                    result = new Piece(new Vector(cur1 | (BigInteger)result1.X, cur1 | (BigInteger)result1.Y, cur1 | (BigInteger)result1.Z));
                                    continue;
                                }

                                if (!isNum1 && isNum2)
                                {
                                    Vector cur1 = (Vector)cur.Value;
                                    BigInteger result1 = (BigInteger)(Fraction)result.Value;
                                    result = new Piece(new Vector((BigInteger)cur1.X | result1, (BigInteger)cur1.Y | result1, (BigInteger)cur1.Z | result1));
                                    continue;
                                }

                                {
                                    Vector cur1 = (Vector)cur.Value;
                                    Vector result1 = (Vector)result.Value;
                                    result = new Piece(new Vector((BigInteger)cur1.X | (BigInteger)result1.X, (BigInteger)cur1.Y | (BigInteger)result1.Y, (BigInteger)cur1.Z | (BigInteger)result1.Z));
                                }
                            }

                            return result;
                        }
                    case "bxor":
                        {
                            // Bitwise XOR
                            if (Formulas.Count == 0)
                                return new Piece(0);

                            // Initialize
                            workOutput += CalcFormula(0, out Piece result);

                            for (int i = 1; i < Formulas.Count; i++)
                            {
                                workOutput += CalcFormula(i, out Piece cur);

                                bool isNum1 = cur.Type == "num";
                                bool isNum2 = result.Type == "num";

                                if (isNum1 && isNum2)
                                {
                                    result = new Piece((BigInteger)(Fraction)cur.Value ^ ((BigInteger)(Fraction)result.Value));
                                    continue;
                                }

                                if (isNum1 && !isNum2)
                                {
                                    BigInteger cur1 = (BigInteger)(Fraction)cur.Value;
                                    Vector result1 = (Vector)result.Value;
                                    result = new Piece(new Vector(cur1 ^ (BigInteger)result1.X, cur1 ^ (BigInteger)result1.Y, cur1 ^ (BigInteger)result1.Z));
                                    continue;
                                }

                                if (!isNum1 && isNum2)
                                {
                                    Vector cur1 = (Vector)cur.Value;
                                    BigInteger result1 = (BigInteger)(Fraction)result.Value;
                                    result = new Piece(new Vector((BigInteger)cur1.X ^ result1, (BigInteger)cur1.Y ^ result1, (BigInteger)cur1.Z ^ result1));
                                    continue;
                                }

                                {
                                    Vector cur1 = (Vector)cur.Value;
                                    Vector result1 = (Vector)result.Value;
                                    result = new Piece(new Vector((BigInteger)cur1.X ^ (BigInteger)result1.X, (BigInteger)cur1.Y ^ (BigInteger)result1.Y, (BigInteger)cur1.Z ^ (BigInteger)result1.Z));
                                }
                            }

                            return result;
                        }
                    case "bshift":
                        {
                            // Bitshift
                            workOutput += CalcFormula(0, out Piece piece);
                            workOutput += CalcFormula(1, out Piece bits);

                            bool isNum1 = piece.Type == "num";
                            bool isNum2 = bits.Type == "num";

                            if (isNum1 && isNum2)
                            {
                                BigInteger num = (BigInteger)(Fraction)piece.Value;
                                int shift = (int)(Fraction)bits.Value;

                                return new Piece(num << shift);
                            }

                            if (isNum1 && !isNum2)
                            {
                                BigInteger num = (BigInteger)(Fraction)piece.Value;
                                Vector shiftVec = (Vector)bits.Value;
                                int shiftx = (int)shiftVec.X;
                                int shifty = (int)shiftVec.Y;
                                int shiftz = (int)shiftVec.Z;

                                return new Piece(new Vector(num << shiftx, num << shifty, num << shiftz));
                            }

                            if (!isNum1 && isNum2)
                            {
                                Vector num = (Vector)piece.Value;
                                int shift = (int)(Fraction)bits.Value;
                                return new Piece(new Vector((BigInteger)num.X << shift, (BigInteger)num.Y << shift, (BigInteger)num.Z << shift));
                            }

                            {
                                Vector num = (Vector)piece.Value;
                                Vector shiftVec = (Vector)bits.Value;
                                int shiftx = (int)shiftVec.X;
                                int shifty = (int)shiftVec.Y;
                                int shiftz = (int)shiftVec.Z;

                                return new Piece(new Vector((BigInteger)num.X << shiftx, (BigInteger)num.Y << shifty, (BigInteger)num.Z << shiftz));
                            }
                        }
                    case "bnot":
                        {
                            // Bitwise NOT
                            workOutput += CalcFormula(0, out Piece piece);
                            int? bits = null;

                            if (Formulas.Count == 2)
                            {
                                workOutput += CalcFormula(1, out Piece bitPiece);
                                if (bitPiece.Type == "num")
                                    bits = (int)(Fraction)bitPiece.Value;
                            }

                            if (piece.Type == "num")
                            {
                                BigInteger num = (BigInteger)(Fraction)piece.Value;
                                return new Piece(num ^ (BigInteger.Pow(2, bits ?? (int)Math.Floor(BigInteger.Log(num, 2) + 1)) - 1));
                            }

                            Vector vec = (Vector)piece.Value;
                            BigInteger x = (BigInteger)vec.X;
                            BigInteger y = (BigInteger)vec.Y;
                            BigInteger z = (BigInteger)vec.Z;
                            return new Piece(
                                new Vector(
                                    x ^ (BigInteger.Pow(2, bits ?? (int)Math.Floor(BigInteger.Log(x, 2) + 1)) - 1),
                                    y ^ (BigInteger.Pow(2, bits ?? (int)Math.Floor(BigInteger.Log(y, 2) + 1)) - 1),
                                    z ^ (BigInteger.Pow(2, bits ?? (int)Math.Floor(BigInteger.Log(z, 2) + 1)) - 1)
                                )
                            );
                        }
                }
                return new Piece("");
            }
            catch (FractionDoubleParsingException e)
            {
                throw new FunctionException(FuncName, e.Message);
            }
        }

        private static int FindBreakoff(string input, int index, bool xIsVar)
        {
            int openCount = 0;
            for (int i = index; i < input.Length; i++)
            {
                char c = input[i];
                switch (c)
                {
                    case '(': openCount++; break;
                    case ')':
                        openCount--;
                        if (openCount < 0)
                            return i - 1;
                        break;
                    default:
                        if ((!xIsVar || c != 'x') && Matching.IsOperator(c.ToString()) && openCount <= 0)
                            return i - 1;
                        break;
                }
            }

            return input.Length - 1;
        }

        public override string ToString()
        {
            switch (FuncName)
            {
                case "min":
                    {
                        string s = "min(";
                        bool first = true;
                        foreach (Formula formula in Formulas)
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
                        foreach (Formula formula in Formulas)
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
                    return Formulas.Count == 1 ?
                        $"log({Formulas[0].formula})" :
                        $"log({Formulas[1].formula}, base = {Formulas[0].formula})";
                case "round":
                    return Formulas.Count == 1 ?
                        $"round({Formulas[0].formula})" :
                        $"round({Formulas[0].formula}, digits = {Formulas[1].formula})";
                case "sum":
                    return $"∑({argVars[0]}={Formulas[0].formula} to {Formulas[1].formula}, a({argVars[0]})={Formulas[2].formula})";
                case "prod":
                    return $"∏({argVars[0]}={Formulas[0].formula} to {Formulas[1].formula}, a({argVars[0]})={Formulas[2].formula})";
            }

            return OriginalString;
        }
    }
}