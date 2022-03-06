using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Calculator
{
    internal class Formula
    {
        public string formula = "";

        private static readonly Fraction
            deg2rad = Fraction.PI / 180,
            rad2deg = 180 / Fraction.PI,
            sciNotMinThreshold = new Fraction(1, 1000000),
            sciNotMaxThreshold = new Fraction(1000000000000000, 1);
        internal const int DEC_DIGIT_DISPLAY = 10;
        private static Regex pieceRegex;
        internal Dictionary<string, Piece> vars;


        public Formula(string formula)
        {
            this.formula = formula.Replace(" ", "").Replace("\t", "");
            vars = new Dictionary<string, Piece>();
            ConstructPieceRegex();
        }

        public Formula(string formula, Dictionary<string, Piece> vars)
        {
            this.formula = formula.Replace(" ", "").Replace("\t", "");
            this.vars = vars;
            ConstructPieceRegex();
        }

        private void ConstructPieceRegex()
        {
            if (vars.Any())
            {
                string regex = Matching.ConstructPieceRegex(vars.Keys.ToArray());
                pieceRegex = new Regex(regex, RegexOptions.Multiline);
            }
            else
            {
                pieceRegex = Matching.RE_DefaultPieces;
            }
        }

        public void SetVar(string key, Piece value)
        {
            vars[key] = value;
            ConstructPieceRegex();
        }

        private static string ToStringException(Piece piece)
        {
            switch (piece.Type)
            {
                default:
                    return (string)piece.Value;
                case "func1":
                    return (string)piece.Value;
                case "num":
                    return ((Fraction)piece.Value).ToFracString();
                case "vec":
                    Vector vec = (Vector)piece.Value;
                    string x = vec.X.ToFracString();
                    string y = vec.Y.ToFracString();
                    string z = vec.Z.ToFracString();
                    return $"<{x}, {y}, {z}>";
                case "parse vec":
                    string[] components = (string[])piece.Value;
                    return $"<{components[0]}, {components[1]}, {components[2]}>";
                case "func":
                    return ((Function)piece.Value).ToString();
            }
        }

        internal static string ToString(Fraction n, bool round, bool deci)
        {
            if (deci)
            {
                // Output decimal
                if (n.Numerator == 0)
                {
                    return "0";
                }

                if (Fraction.Abs(n) < sciNotMinThreshold || Fraction.Abs(n) >= sciNotMaxThreshold)
                {
                    // Convert to scientific notation
                    // Decimal
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
                }

                // Output as is
                if (round)
                {
                    return Fraction.Round(n, DEC_DIGIT_DISPLAY).ToString(DEC_DIGIT_DISPLAY);
                }
                return n.ToString();
            }

            // Output fraction
            if (round)
            {
                return Fraction.Round(n, DEC_DIGIT_DISPLAY).ToFracString();
            }
            return n.ToFracString();
        }

        private static string ToString(Piece piece)
        {
            if (piece.IsVar)
            {
                return piece.VarName;
            }
            switch (piece.Type)
            {
                default:
                    return (string)piece.Value;
                case "func1":
                    if (piece.Value == "neg")
                    {
                        return "-";
                    }
                    return (string)piece.Value;
                case "num":
                    return ToString((Fraction)piece.Value, true, true);
                case "vec":
                    Vector vec = (Vector)piece.Value;
                    return $"<{ToString(vec.X, true, true)}, {ToString(vec.Y, true, true)}, {ToString(vec.Z, true, true)}>";
                case "parse vec":
                    string[] components = (string[])piece.Value;
                    return $"<{components[0]}, {components[1]}, {components[2]}>";
                case "func":
                    return ((Function)piece.Value).ToString();
            }
        }

        private static string AnswerToString(Piece piece, bool final)
        {
            switch (piece.Type)
            {
                default:
                    return (string)piece.Value;
                case "func1":
                    if (piece.Value == "neg")
                    {
                        return "-";
                    }
                    return (string)piece.Value;
                case "num":
                    return ToString((Fraction)piece.Value, final, final);
                case "const":
                    return ToString(piece.ConstValue, final, final);
                case "vec":
                    Vector vec = (Vector)piece.Value;
                    string x = ToString(vec.X, final, final);
                    string y = ToString(vec.Y, final, final);
                    string z = ToString(vec.Z, final, final);
                    return $"<{x}, {y}, {z}>";
                case "parse vec":
                    string[] components = (string[])piece.Value;
                    return $"<{components[0]}, {components[1]}, {components[2]}>";
                case "func":
                    return ((Function)piece.Value).ToString();
            }
        }

        private List<Piece> String2Infix(string formula, out string error)
        {
            error = "";
            List<Piece> pieces = new List<Piece>();
            int lastIndex = 0;
            foreach (Match match in pieceRegex.Matches(formula))
            {
                if (match.Index < lastIndex)
                {
                    continue;
                }

                if (match.Index != lastIndex)
                {
                    error = $"Error: Unexpected term \"{formula.Substring(lastIndex, match.Index - lastIndex)}\"";
                    return pieces;
                }

                if (Matching.IsFunc(match.Value))
                {
                    // This is a function
                    try
                    {
                        pieces.Add(new Piece(new Function(formula.Substring(match.Index), vars, out int stop)));
                        lastIndex = stop + match.Index;
                        continue;
                    }
                    catch (FunctionException ex)
                    {
                        error = $"Error parsing {ex.FunctionName}: {ex.Message}";
                        return pieces;
                    }
                }

                lastIndex += match.Length;
                string value = match.Value;
                if (vars.ContainsKey(value))
                {
                    pieces.Add(vars[value]);
                }
                else
                {
                    try
                    {
                        pieces.Add(new Piece(match.Value));
                    }
                    catch (FractionDoubleParsingException)
                    {
                        error = $"Error: {match.Value} is too small or large.";
                        return pieces;
                    }
                }
            }
            if (lastIndex != formula.Length)
            {
                error = $"Error: Unexpected term \"{formula.Substring(lastIndex)}\"";
                return pieces;
            }

            // Post processing
            int i = 0;
            while (i < pieces.Count - 1)
            {
                Piece piece = pieces[i];

                if (piece.Value.Equals("-"))
                {
                    if (i == 0)
                    {
                        pieces[i] = new Piece("neg");
                        piece = pieces[i];
                    }
                    else
                    {
                        Piece last = pieces[i - 1];
                        if (last.Type == "op" ||
                            last.Value.Equals("(") ||
                            last.Type == "func1")
                        {
                            pieces[i] = new Piece("neg");
                            piece = pieces[i];
                        }
                    }
                    if (piece.Value.Equals("neg"))
                    {
                        if (pieces[i + 1].Type == "num")
                        {
                            pieces[i] = new Piece(-(Fraction)pieces[i + 1].Value);
                            piece = pieces[i];
                            pieces.RemoveAt(i + 1);
                        }
                        else if (pieces[i + 1].Type == "vec")
                        {
                            pieces[i] = new Piece(-(Vector)pieces[i + 1].Value);
                            piece = pieces[i];
                            pieces.RemoveAt(i + 1);
                        }
                    }
                }

                // Add unwritten multiplication
                if (i + 1 < pieces.Count &&
                    (piece.Value.Equals(")") ||
                    piece.Type == "num" ||
                    piece.Type == "vec" ||
                    piece.Type == "parse vec" ||
                    piece.Type == "const" ||
                    piece.Type == "func"))
                {
                    // Check next piece
                    Piece nextPiece = pieces[i + 1];
                    if (nextPiece.Type == "num" ||
                        nextPiece.Type == "func1" ||
                        nextPiece.Type == "vec" ||
                        nextPiece.Type == "parse vec" ||
                        nextPiece.Type == "const" ||
                        nextPiece.Type == "func" ||
                        nextPiece.Value.Equals("("))
                    {
                        // Insert multiplication
                        if (i + 2 < pieces.Count && pieces[i + 2].Value.Equals("^"))
                        {
                            // Exponent
                            // Don't add parentheses
                            pieces.Insert(i + 1, new Piece("*"));
                            i++;
                        }
                        else
                        {
                            int open = i;
                            int close = i + 2;

                            // Find close pos
                            if (nextPiece.Type == "func1")
                            {
                                // Func1 to the right
                                // Check if it has parentheses
                                if (i + 2 < pieces.Count && pieces[i + 2].Value.Equals("("))
                                {
                                    // Func1 has parentheses
                                    // Find closing parenthesis
                                    int temp = Matching.FindClosingParenthesis(pieces, i + 2);
                                    if (temp != -1)
                                    {
                                        close = temp + 1;
                                    }
                                }
                                else
                                {
                                    // Func1 has no parentheses
                                    // Check for other partnered terms
                                    // Preserve other func1s and their parentheses and groups
                                    // Search for operators to break on
                                    bool lastWasFunc1 = false;
                                    close--;
                                    while (close < pieces.Count)
                                    {
                                        if (lastWasFunc1)
                                        {
                                            // This piece belongs to the func1
                                            close++;
                                            continue;
                                        }
                                        Piece piece2 = pieces[close];
                                        if (piece2.Type == "op")
                                        {
                                            // Found close pos
                                            break;
                                        }
                                        else if (piece2.Type == "func1")
                                        {
                                            lastWasFunc1 = true;
                                        }
                                        else if (piece2.Value.Equals("("))
                                        {
                                            // Find closing parenthesis
                                            int temp = Matching.FindClosingParenthesis(pieces, close);
                                            if (temp != -1)
                                            {
                                                close = temp;
                                            }
                                        }
                                        close++;
                                    }
                                }
                            }
                            else if (nextPiece.Value.Equals("("))
                            {
                                // Not a func1 to the right
                                // Parentheses to the right
                                // Find close pos
                                int temp = Matching.FindClosingParenthesis(pieces, i + 1);
                                if (temp != -1)
                                {
                                    close = temp + 1;
                                }
                            }

                            if (close < pieces.Count && pieces[close].Value.Equals("^"))
                            {
                                // Don't wrap in parentheses
                                pieces.Insert(i + 1, new Piece("*"));
                            }
                            else
                            {
                                // Find open pos
                                if (piece.Value.Equals(")"))
                                {
                                    // Parentheses to the left
                                    // Find opening parenthesis
                                    int temp = Matching.FindOpeningParenthesis(pieces, i);
                                    if (temp != -1)
                                    {
                                        open = temp;
                                    }
                                }

                                pieces.Insert(close, new Piece(")"));
                                pieces.Insert(i + 1, new Piece("*"));
                                pieces.Insert(open, new Piece("("));
                            }
                        }
                    }
                }
                i++;
            }

            return CleanupInfix(pieces);
        }

        private static bool IsCommunitive(Piece piece)
        {
            // Check if the operator is communitive
            if (piece.Type == "op")
            {
                switch ((string)piece.Value)
                {
                    case "+":
                    case "*":
                    case ".":
                        return true;
                }
            }
            return false;
        }

        private static List<Piece> CleanupInfix(List<Piece> pieces)
        {
            //Remove unnecessary parentheses
            int i = 0;
            while (i < pieces.Count)
            {
                Piece piece = pieces[i];
                if (piece.Value.Equals("("))
                {
                    if (i > 0 && pieces[i - 1].Type == "func1" && pieces[i - 1].Value != "neg")
                    {
                        // The previous piece should always preserve parentheses for following terms
                        i++;
                        continue;
                    }

                    // Found opening parenthesis
                    // Now find closing parenthesis and keep track of the highest precedence
                    int openCount = 1;
                    int minPrecedence = 100;
                    for (int close = i + 1; close < pieces.Count; close++)
                    {
                        Piece piece2 = pieces[close];
                        if (piece2.Value.Equals("("))
                        {
                            openCount++;
                        }
                        else if (piece2.Value == ")")
                        {
                            openCount--;
                            if (openCount == 0)
                            {
                                // Found closing parenthesis
                                // Compare max precedence to the pieces before and after
                                if (minPrecedence != 100)
                                {
                                    // Before
                                    if (i > 0)
                                    {
                                        Piece prevPiece = pieces[i - 1];
                                        if (minPrecedence < prevPiece.Precedence ||
                                            (minPrecedence == prevPiece.Precedence && !IsCommunitive(prevPiece)))
                                        {
                                            // Prev piece has a higher precedence than the inners
                                            // OR they have the same precedence but the prev piece is not a communitive operator
                                            // Keep parentheses
                                            break;
                                        }
                                    }

                                    // After
                                    if (close + 1 < pieces.Count && minPrecedence < pieces[close + 1].Precedence)
                                    {
                                        // Keep parentheses
                                        break;
                                    }
                                }

                                // Remove these parentheses
                                pieces.RemoveAt(close);
                                pieces.RemoveAt(i);
                                i--;
                                break;
                            }
                        }
                        else if (openCount == 1 && piece2.Precedence != -1)
                        {
                            minPrecedence = Math.Min(minPrecedence, piece2.Precedence);
                        }
                    }
                }
                i++;
            }

            return pieces;
        }

        private static string Postfix2Infix(List<Piece> postfix)
        {
            Stack<List<Piece>> stack = new Stack<List<Piece>>();
            foreach (Piece piece in postfix)
            {
                if (piece.Type == "op")
                {
                    if (piece.Value.Equals("neg"))
                    {
                        // Negation
                        if (stack.Count > 0)
                        {
                            List<Piece> pieces = new List<Piece> { piece };
                            pieces.AddRange(stack.Pop());
                            stack.Push(pieces);
                        }
                    }
                    else if (stack.Count == 1)
                    {
                        List<Piece> pieces = new List<Piece>
                        {
                            new Piece("("),
                            piece
                        };
                        pieces.AddRange(stack.Pop());
                        pieces.Add(new Piece(")"));
                        stack.Push(pieces);
                    }
                    else if (stack.Count > 1)
                    {
                        List<Piece> op2 = stack.Pop();
                        List<Piece> op1 = stack.Pop();

                        List<Piece> pieces = new List<Piece> { new Piece("(") };
                        pieces.AddRange(op1);
                        pieces.Add(piece);
                        pieces.AddRange(op2);
                        pieces.Add(new Piece(")"));

                        stack.Push(pieces);
                    }
                    else
                    {
                        stack.Push(new List<Piece> { piece });
                    }
                }
                else if (piece.Type == "func1")
                {
                    if (stack.Count != 0)
                    {
                        List<Piece> pieces = new List<Piece> { new Piece("("), piece };
                        pieces.AddRange(stack.Pop());
                        pieces.Add(new Piece(")"));

                        stack.Push(pieces);
                    }
                }
                else
                {
                    stack.Push(new List<Piece> { piece });
                }
            }
            if (stack.Count == 0)
            {
                return "";
            }

            {
                List<Piece> pieces = CleanupInfix(stack.Peek());

                // Convert to string
                string s = "";
                foreach (Piece piece in pieces)
                {
                    s += ToString(piece);
                }

                return s;
            }
        }

        private static List<Piece> Infix2Postfix(List<Piece> infix, out string error)
        {
            error = "";
            Stack<Piece> stack = new Stack<Piece>();
            List<Piece> postfix = new List<Piece>();
            for (int i = 0; i < infix.Count; i++)
            {
                Piece piece = infix[i];
                if (piece.IsOperand || piece.Type == "func")
                {
                    // Operand
                    // Push to output
                    postfix.Add(piece);
                }
                else if (piece.Type == "func1" || piece.Value.Equals("("))
                {
                    // Func1 or open parenthesis
                    // Push to stack
                    stack.Push(piece);
                }
                else if (piece.Value.Equals(")"))
                {
                    // Close parenthesis
                    // Pop and output until (
                    bool foundOpen = false;
                    while (stack.Count > 0)
                    {
                        Piece popped = stack.Pop();
                        if (popped.Value.Equals("("))
                        {
                            foundOpen = true;
                            break;
                        }
                        postfix.Add(popped);
                    }
                    if (!foundOpen)
                    {
                        error = "Error: Unopened closing parenthesis";
                        return null;
                    }
                }
                else if (piece.Type == "op")
                {
                    // Operator or func1
                    // Pop and output while >= precedence and not ( then push to stack
                    Piece peeked;
                    while (stack.Count > 0 &&
                        (peeked = stack.Peek()).Precedence >= piece.Precedence &&
                        !peeked.Value.Equals("("))
                    {
                        postfix.Add(stack.Pop());
                    }
                    stack.Push(piece);
                }
            }

            // Pop remaining operators and func1 to output
            while (stack.Count > 0)
            {
                Piece piece = stack.Pop();
                if (piece.Value.Equals("("))
                {
                    error = "Error: Unclosed open parenthesis";
                    return null;
                }
                postfix.Add(piece);
            }

            return postfix;
        }

        private static string Postfix2Infix(Stack<Piece> stack, List<Piece> pieces, int index)
        {
            List<Piece> postfix = new List<Piece>();

            foreach (Piece piece in stack)
            {
                postfix.Insert(0, piece);
            }

            for (int j = index; j < pieces.Count; j++)
            {
                postfix.Add(pieces[j]);
            }

            return Postfix2Infix(postfix);
        }

        private static string Postfix2String(List<Piece> pieces)
        {
            string s = "";
            foreach (Piece piece in pieces)
            {
                if (s.Length > 0)
                {
                    s += $" {ToString(piece)}";
                }
                else
                {
                    s += ToString(piece);
                }
            }
            return s;
        }

        public string Calculate()
        {
            return Calculate(out _, false, false, false);
        }

        public string Calculate(out string workOutput)
        {
            return Calculate(out workOutput, false, false, false);
        }

        public string Calculate(out string workOutput, bool isSub, bool final, bool minWork)
        {
            workOutput = "";
            List<Piece> pieces;
            {
                pieces = String2Infix(formula, out string error);
                if (error.Length > 0)
                {
                    workOutput += $"{error}\n";
                    return error;
                }
            }

            if (!isSub && !minWork)
            {
                workOutput += "Adjusted: ";
                foreach (Piece piece in pieces)
                {
                    try
                    {
                        workOutput += ToString(piece);
                    }
                    catch (FractionDoubleParsingException)
                    {
                        string error = $"Error: {ToStringException(piece)} is too small or large.";
                        workOutput += $"{error}\n";
                        return error;
                    }
                }
                workOutput += "\n";
            }

            // Parse vectors
            {
                for (int i = 0; i < pieces.Count; i++)
                {
                    if (pieces[i].Type == "parse vec")
                    {
                        string[] components = (string[])pieces[i].Value;
                        Fraction[] newComponents = new Fraction[3];
                        for (int j = 0; j < 3; j++)
                        {
                            string newComponent = new Formula(components[j], vars).Calculate(out string work, true, false, true);
                            work = work.Trim();
                            if (work.Length > 0 && !Matching.RE_GenericNum.IsMatch(components[j]))
                            {
                                string pre = "";
                                switch (j)
                                {
                                    case 0:
                                        pre = "x: ";
                                        break;
                                    case 1:
                                        pre = "y: ";
                                        break;
                                    case 2:
                                        pre = "z: ";
                                        break;
                                }

                                work = $"{components[j]}\n{work}\n{Matching.Answer2Decimal(newComponent)}";
                                foreach (string line in work.Split('\n'))
                                {
                                    if (line.Length > 0)
                                    {
                                        workOutput += pre + line + '\n';
                                    }
                                }
                                workOutput += '\n';
                            }
                            if (Fraction.TryParse(newComponent, out Fraction num))
                            {
                                newComponents[j] = num;
                            }
                            else
                            {
                                workOutput += "Error: Unparsable vector component\n";
                                return "Error: Unparsable vector component";
                            }
                        }
                        pieces[i] = new Piece(new Vector(newComponents[0], newComponents[1], newComponents[2]));
                    }
                }
            }

            // Convert from infix to postfix
            {
                pieces = Infix2Postfix(pieces, out string error);
                if (error.Length > 0)
                {
                    workOutput += $"{error}\n";
                    return error;
                }
            }

            // Display postfix
            if (!isSub)
            {
                workOutput += $"Postfix: {Postfix2String(pieces)}\n";
            }

            // Calculate
            Stack<Piece> stack = new Stack<Piece>();
            {
                bool firstLine = true;
                for (int i = 0; i < pieces.Count; i++)
                {
                    Piece piece = pieces[i];

                    // Calculate
                    if (piece.IsOperand)
                    {
                        // Operand
                        stack.Push(piece);
                    }
                    else if (piece.Type == "op")
                    {
                        // Operator
                        workOutput += Postfix2Infix(stack, pieces, i) + '\n';

                        if (stack.Count < 2)
                        {
                            string error = $"Error: Not enough operands for {ToString(piece)}";
                            workOutput += error + '\n';
                            return error;
                        }

                        Piece num2 = stack.Pop();
                        Piece num1 = stack.Pop();

                        if (num1.Type == "const")
                        {
                            num1 = new Piece(num1.ConstValue);
                        }
                        if (num2.Type == "const")
                        {
                            num2 = new Piece(num2.ConstValue);
                        }

                        bool isNum1 = num1.Type == "num";
                        bool isNum2 = num2.Type == "num";

                        if (!isNum1 && num1.Type != "vec")
                        {
                            string error = $"Error: Cannot perform arithmetic on {ToString(num1)}";
                            workOutput += error + '\n';
                            return error;
                        }
                        if (!isNum2 && num2.Type != "vec")
                        {
                            string error = $"Error: Cannot perform arithmetic on {ToString(num2)}";
                            workOutput += error + '\n';
                            return error;
                        }

                        switch ((string)piece.Value)
                        {
                            case "+":
                                if (isNum1 && isNum2)
                                {
                                    stack.Push(new Piece((Fraction)num1.Value + (Fraction)num2.Value));
                                }
                                else if (isNum1 && !isNum2)
                                {
                                    stack.Push(new Piece((Fraction)num1.Value + (Vector)num2.Value));
                                }
                                else if (!isNum1 && isNum2)
                                {
                                    stack.Push(new Piece((Vector)num1.Value + (Fraction)num2.Value));
                                }
                                else
                                {
                                    stack.Push(new Piece((Vector)num1.Value + (Vector)num2.Value));
                                }
                                break;
                            case "-":
                                if (isNum1 && isNum2)
                                {
                                    stack.Push(new Piece((Fraction)num1.Value - (Fraction)num2.Value));
                                }
                                else if (isNum1 && !isNum2)
                                {
                                    stack.Push(new Piece((Fraction)num1.Value - (Vector)num2.Value));
                                }
                                else if (!isNum1 && isNum2)
                                {
                                    stack.Push(new Piece((Vector)num1.Value - (Fraction)num2.Value));
                                }
                                else
                                {
                                    stack.Push(new Piece((Vector)num1.Value - (Vector)num2.Value));
                                }
                                break;
                            case "*":
                                if (isNum1 && isNum2)
                                {
                                    stack.Push(new Piece((Fraction)num1.Value * (Fraction)num2.Value));
                                }
                                else if (isNum1 && !isNum2)
                                {
                                    stack.Push(new Piece((Fraction)num1.Value * (Vector)num2.Value));
                                }
                                else if (!isNum1 && isNum2)
                                {
                                    stack.Push(new Piece((Vector)num1.Value * (Fraction)num2.Value));
                                }
                                else
                                {
                                    stack.Push(new Piece((Vector)num1.Value * (Vector)num2.Value));
                                }
                                break;
                            case "/":
                                if (isNum2)
                                {
                                    if ((Fraction)num2.Value == 0)
                                    {
                                        string error = $"Error: Division by 0";
                                        workOutput += error + '\n';
                                        return error;
                                    }
                                }
                                else
                                {
                                    Vector vec = (Vector)num2.Value;
                                    if (vec.X == 0 || vec.Y == 0 || vec.Z == 0)
                                    {
                                        string error = $"Error: Division by 0";
                                        workOutput += error + '\n';
                                        return error;
                                    }
                                }
                                if (isNum1 && isNum2)
                                {
                                    stack.Push(new Piece((Fraction)num1.Value / (Fraction)num2.Value));
                                }
                                else if (isNum1 && !isNum2)
                                {
                                    stack.Push(new Piece((Fraction)num1.Value / (Vector)num2.Value));
                                }
                                else if (!isNum1 && isNum2)
                                {
                                    stack.Push(new Piece((Vector)num1.Value / (Fraction)num2.Value));
                                }
                                else
                                {
                                    stack.Push(new Piece((Vector)num1.Value / (Vector)num2.Value));
                                }
                                break;
                            case "x":
                                if (!isNum1 && !isNum2)
                                {
                                    stack.Push(new Piece(((Vector)num1.Value).Cross((Vector)num2.Value)));
                                }
                                else
                                {
                                    string error = $"Error: Cannot get cross product of {ToString(num1)} and {ToString(num2)}";
                                    workOutput += error + '\n';
                                    return error;
                                }
                                break;
                            case ".":
                                if (!isNum1 && !isNum2)
                                {
                                    stack.Push(new Piece(((Vector)num1.Value).Dot((Vector)num2.Value)));
                                }
                                else
                                {
                                    string error = $"Error: Cannot get dot product of {ToString(num1)} and {ToString(num2)}";
                                    workOutput += error + '\n';
                                    return error;
                                }
                                break;
                            case "%":
                                if (isNum1 && isNum2)
                                {
                                    stack.Push(new Piece((Fraction)num1.Value % (Fraction)num2.Value));
                                }
                                else if (isNum1 && !isNum2)
                                {
                                    stack.Push(new Piece((Fraction)num1.Value % (Vector)num2.Value));
                                }
                                else if (!isNum1 && isNum2)
                                {
                                    stack.Push(new Piece((Vector)num1.Value % (Fraction)num2.Value));
                                }
                                else
                                {
                                    stack.Push(new Piece((Vector)num1.Value % (Vector)num2.Value));
                                }
                                break;
                            case "^":
                                try
                                {
                                    if (isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece(Fraction.Pow((Fraction)num1.Value, (Fraction)num2.Value)));
                                    }
                                    else if (isNum1 && !isNum2)
                                    {
                                        stack.Push(new Piece(Vector.Pow((Fraction)num1.Value, (Vector)num2.Value)));
                                    }
                                    else if (!isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece(Vector.Pow((Vector)num1.Value, (Fraction)num2.Value)));
                                    }
                                    else
                                    {
                                        stack.Push(new Piece(Vector.Pow((Vector)num1.Value, (Vector)num2.Value)));
                                    }
                                }
                                catch
                                {
                                    string error = $"Error: Cannot raise {ToString(num1)} to the {ToString(num2)} power";
                                    workOutput += error + '\n';
                                    return error;
                                }
                                break;
                        }
                    }
                    else if (piece.Type == "func1")
                    {
                        // Func1
                        if (firstLine)
                        {
                            firstLine = false;
                            workOutput += Postfix2Infix(stack, pieces, i);
                        }
                        else
                        {
                            workOutput += Postfix2Infix(stack, pieces, i) + '\n';
                        }

                        if (stack.Count == 0)
                        {
                            string error = $"Error: Not enough operands for {ToString(piece)}";

                            workOutput += error + '\n';
                            return error;
                        }

                        Piece num = stack.Pop();
                        if (num.Type == "const")
                        {
                            num = new Piece(num.ConstValue);
                        }

                        bool isNum = num.Type == "num";

                        if (!isNum && num.Type != "vec")
                        {
                            string error = $"Error: Cannot perform {ToString(piece)} on {ToString(num)}";
                            workOutput += error + '\n';
                            return error;
                        }

                        switch ((string)piece.Value)
                        {
                            case "sin":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Fraction.Sin((Fraction)num.Value)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Vector.Sin((Vector)num.Value)));
                                }
                                break;
                            case "asin":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Fraction.Asin((Fraction)num.Value)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Vector.Asin((Vector)num.Value)));
                                }
                                break;
                            case "cos":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Fraction.Cos((Fraction)num.Value)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Vector.Cos((Vector)num.Value)));
                                }
                                break;
                            case "acos":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Fraction.Acos((Fraction)num.Value)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Vector.Acos((Vector)num.Value)));
                                }
                                break;
                            case "tan":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Fraction.Tan((Fraction)num.Value)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Vector.Tan((Vector)num.Value)));
                                }
                                break;
                            case "atan":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Fraction.Atan((Fraction)num.Value)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Vector.Atan((Vector)num.Value)));
                                }
                                break;
                            case "abs":
                                if (isNum)
                                {
                                    Fraction temp = (Fraction)num.Value;
                                    if (temp < 0)
                                    {
                                        stack.Push(new Piece(-temp));
                                    }
                                    else
                                    {
                                        stack.Push(num);
                                    }
                                }
                                else
                                {
                                    stack.Push(new Piece(Vector.Abs((Vector)num.Value)));
                                }
                                break;
                            case "floor":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Fraction.Floor((Fraction)num.Value)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Vector.Floor((Vector)num.Value)));
                                }
                                break;
                            case "ceil":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Fraction.Ceiling((Fraction)num.Value)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Vector.Ceiling((Vector)num.Value)));
                                }
                                break;
                            case "round":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Fraction.Round((Fraction)num.Value)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Vector.Round((Vector)num.Value)));
                                }
                                break;
                            case "sqrt":
                                try
                                {
                                    if (isNum)
                                    {
                                        stack.Push(new Piece(Fraction.Pow((Fraction)num.Value, new Fraction(1, 2))));
                                    }
                                    else
                                    {
                                        stack.Push(new Piece(Vector.Pow((Vector)num.Value, new Fraction(1, 2))));
                                    }
                                }
                                catch
                                {
                                    string error = $"Error: Cannot square root {num}";
                                    workOutput += error + '\n';
                                    return error;
                                }
                                break;
                            case "deg":
                                if (isNum)
                                {
                                    stack.Push(new Piece((Fraction)num.Value * rad2deg));
                                }
                                else
                                {
                                    stack.Push(new Piece((Vector)num.Value * rad2deg));
                                }
                                break;
                            case "rad":
                                if (isNum)
                                {
                                    stack.Push(new Piece((Fraction)num.Value * deg2rad));
                                }
                                else
                                {
                                    stack.Push(new Piece((Vector)num.Value * deg2rad));
                                }
                                break;
                            case "neg":
                                // Negate
                                if (isNum)
                                {
                                    stack.Push(new Piece(-(Fraction)num.Value));
                                }
                                else
                                {
                                    stack.Push(new Piece(-(Vector)num.Value));
                                }
                                break;
                            case "sign":
                                // Sign
                                if (isNum)
                                {
                                    stack.Push(new Piece(Fraction.Sign((Fraction)num.Value)));
                                }
                                else
                                {
                                    Vector vec = (Vector)num.Value;
                                    stack.Push(new Piece(new Vector(
                                        Fraction.Sign(vec.X),
                                        Fraction.Sign(vec.Y),
                                        Fraction.Sign(vec.Z))));
                                }
                                break;
                            case "sigfig4":
                                // Round to 4 sigfigs
                                Func<Fraction, Fraction> sigfig = n =>
                                {
                                    if (n == 0)
                                    {
                                        return 0;
                                    }

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

                                    Fraction mult2 = Fraction.Pow(10, -digits - 1);
                                    Fraction sci = n * mult2;
                                    return Fraction.Round(sci, 4) / mult2;
                                };

                                if (isNum)
                                {
                                    stack.Push(new Piece(sigfig((Fraction)num.Value)));
                                }
                                else
                                {
                                    Vector vec = (Vector)num.Value;
                                    stack.Push(new Piece(new Vector(
                                        sigfig(vec.X),
                                        sigfig(vec.Y),
                                        sigfig(vec.Z))));
                                }
                                break;
                        }
                    }
                    else if (piece.Type == "func")
                    {
                        // Function
                        try
                        {
                            stack.Push(((Function)piece.Value).Calculate(out string work));
                            work = work.Trim();
                            if (work.Length > 0)
                            {
                                workOutput += work.Trim() + '\n';
                            }
                        }
                        catch (FunctionException e)
                        {
                            string error = $"Error in {e.FunctionName}: {e.Message}";
                            workOutput += error + '\n';
                            return error;
                        }
                    }
                }
            }

            if (stack.Count > 1)
            {
                string error = "Error: Too many results";
                workOutput += error + '\n';
                return error;
            }

            if (stack.Count == 0)
            {
                string error = "Error: No results";
                workOutput += error + '\n';
                return error;
            }

            return AnswerToString(stack.Peek(), final);
        }
    }
}
