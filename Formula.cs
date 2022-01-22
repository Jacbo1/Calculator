using NewMath;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Calculator
{
    internal class Formula
    {
        private const double deg2rad = Math.PI / 180.0;
        private const double rad2deg = 180.0 / Math.PI;

        // Incredibly easy and simple method to find the pieces and put them into a list in order and allows less strict input formatting
        private static Regex pieceRegex = new Regex(@"(<\S+?,\S+?,\S+?>|\+|-|\*|\/|\^|%|sin|asin|cos|acos|tan|atan|rad|deg|abs|floor|ceil|round|sqrt|\(|\)|([0-9]*\.)?[0-9]+|e|pi|x|\.)");

        public string formula = "";

        public Formula (string formula)
        {
            this.formula = FixFormula(formula);
        }

        private static string FixFormula(string formula)
        {
            string value = formula.Replace(" ", "");
            return value;
        }

        private static string ToString(Piece piece)
        {
            switch (piece.Type)
            {
                default:
                    return (string)piece.Value;
                case "op":
                    if (piece.Value == "neg")
                    {
                        return "-";
                    }
                    return (string)piece.Value;
                case "num":
                    return ((double)piece.Value).ToString();
                case "vec":
                    double3 num = (double3)piece.Value;
                    return $"<{num.x},{num.y},{num.z}>";
                case "parse vec":
                    string[] components = (string[])piece.Value;
                    return $"<{components[0]},{components[1]},{components[2]}>";

            }
        }

        private static string AnswerToString(Piece piece)
        {
            switch (piece.Type)
            {
                default:
                    return (string)piece.Value;
                case "op":
                    if (piece.Value == "neg")
                    {
                        return "-";
                    }
                    return (string)piece.Value;
                case "num":
                    return ((double)piece.Value).ToString();
                case "const":
                    return piece.ConstValue.ToString();
                case "vec":
                    double3 num = (double3)piece.Value;
                    return $"<{num.x}, {num.y}, {num.z}>";
                case "parse vec":
                    string[] components = (string[])piece.Value;
                    return $"<{components[0]}, {components[1]}, {components[2]}>";

            }
        }

        private static List<Piece> String2Infix(string formula)
        {
            List<Piece> pieces = new List<Piece>();
            foreach (Match match in pieceRegex.Matches(formula))
            {
                pieces.Add(new Piece(match.Value));
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
                            last.Value.Equals("("))
                        {
                            pieces[i] = new Piece("neg");
                            piece = pieces[i];
                        }
                    }
                }

                // Add unwritten multiplication
                if (piece.Value.Equals(")") ||
                    piece.Type == "num" ||
                    piece.Type == "vec" ||
                    piece.Type == "parse vec" ||
                    piece.Type == "const")
                {
                    // Check next piece
                    Piece nextPiece = pieces[i + 1];
                    if (nextPiece.Type == "num" ||
                        nextPiece.Type == "func1" ||
                        nextPiece.Type == "vec" ||
                        nextPiece.Type == "parse vec" ||
                        nextPiece.Type == "const" ||
                        nextPiece.Value.Equals("("))
                    {
                        // Insert multiplication
                        if (i + 2 < pieces.Count && pieces[i + 2].Value.Equals("^"))
                        {
                            // Exponent
                            pieces.Insert(i + 1, new Piece("*"));
                            i++;
                        }
                        else
                        {
                            // No exponent
                            if (piece.Value.Equals(")"))
                            {
                                // Parentheses to the left
                                int openCount = 1;
                                for (int j = i - 2; j >= 0; j--)
                                {
                                    Piece piece2 = pieces[j];
                                    if (piece2.Value.Equals(")"))
                                    {
                                        openCount++;
                                    }
                                    else if (piece2.Value.Equals("("))
                                    {
                                        openCount--;
                                        if (openCount == 0)
                                        {
                                            // Found partner parenthesis
                                            pieces.Insert(j, new Piece("("));
                                            pieces.Insert(i + 2, new Piece("*"));
                                            pieces.Insert(i + 4, new Piece(")"));
                                            i += 3;
                                            break;
                                        }
                                    }
                                }
                            }
                            else if (i + 1 < pieces.Count && pieces[i + 1].Value.Equals("("))
                            {
                                // Parentheses to the right
                                int openCount = 1;
                                for (int j = i + 2; j < pieces.Count; j++)
                                {
                                    Piece piece2 = pieces[j];
                                    if (piece2.Value.Equals("("))
                                    {
                                        openCount++;
                                    }
                                    else if (piece2.Value.Equals(")"))
                                    {
                                        openCount--;
                                        if (openCount == 0)
                                        {
                                            // Found partner parenthesis
                                            pieces.Insert(i, new Piece("("));
                                            pieces.Insert(i + 2, new Piece("*"));
                                            pieces.Insert(j + 3, new Piece(")"));
                                            i += 2;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // No parentheses
                                pieces.Insert(i, new Piece("("));
                                pieces.Insert(i + 2, new Piece("*"));
                                pieces.Insert(i + 4, new Piece(")"));
                                i += 3;
                            }
                        }
                    }
                }
                i++;
            }

            return CleanupInfix(pieces);
        }

        private static List<Piece> CleanupInfix(List<Piece> pieces)
        {
            // Remove unnecessary parentheses
            int i = 0;
            while (i < pieces.Count)
            {
                Piece piece = pieces[i];
                if (piece.Value.Equals("("))
                {
                    // Open parenthesis
                    int openCount = 1;
                    int maxPrecedence = 0;
                    for (int j = i + 1; j < pieces.Count; j++)
                    {
                        Piece piece2 = pieces[j];
                        if (piece2.Value.Equals("("))
                        {
                            // Inner open parenthesis
                            openCount++;
                        }
                        else if (piece2.Value.Equals(")"))
                        {
                            openCount--;
                            if (openCount <= 0)
                            {
                                // Found partner parenthesis
                                if (i == 0 ||
                                    (pieces[i - 1].Precedence <= maxPrecedence &&
                                    !pieces[i - 1].Value.Equals("/") &&
                                    !pieces[i - 1].Value.Equals("-")))
                                {
                                    // At start or preceding piece is lower precedence
                                    if (j + 1 == pieces.Count)
                                    {
                                        // End
                                        pieces.RemoveAt(j);
                                        pieces.RemoveAt(i);
                                        i--;
                                    }
                                    else if (pieces[j + 1].Precedence <= maxPrecedence)
                                    {
                                        // Remove parentheses
                                        pieces.RemoveAt(j);
                                        pieces.RemoveAt(i);
                                        i--;
                                    }
                                }
                                break;
                            }
                        }
                        else if (openCount == 1)
                        {
                            maxPrecedence = Math.Max(maxPrecedence, piece2.Precedence);
                        }
                    }
                }
                i++;
            }
            return pieces;
        }

        private static string Postfix2Infix(List<Piece> postfix)
        {
            //PieceGroup pieces = new PieceGroup();
            Stack<List<Piece>> stack = new Stack<List<Piece>> ();
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
                    if (piece.Value == "neg")
                    {
                        s += "-";
                    }
                    else
                    {
                        s += ToString(piece);
                    }
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
                if (piece.IsOperand)
                {
                    // Operand
                    // Push to output
                    postfix.Add(piece);
                }
                else if (piece.Value.Equals("("))
                {
                    // Open parenthesis
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
                else if (piece.Type == "op" || piece.Type == "func1")
                {
                    // Operator or func1
                    // Pop and output while >= precedence and not ( then push to stack
                    Piece peeked;
                    while (stack.Count > 0 &&
                        (peeked = stack.Peek()).Precedence >= piece.Precedence &&
                        !peeked.Value.Equals("(") &&
                        (peeked.Type != "func1" ||
                        piece.Type != "func1"))
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

        public string Calculate(out string workOutput)
        {
            return Calculate(out workOutput, false);
        }

        public string Calculate(out string workOutput, bool isSub)
        {
            workOutput = "";
            List<Piece> pieces = String2Infix(formula);

            if (!isSub)
            {
                workOutput += "Adjusted: ";
                foreach (Piece piece in pieces)
                {
                    workOutput += ToString(piece);
                }
                workOutput += "\n";
            }

            // Parse vectors
            {
                bool parsed = false;
                for (int i = 0; i < pieces.Count; i++)
                {
                    if (pieces[i].Type == "parse vec")
                    {
                        bool shownVec = false;
                        string[] components = (string[])pieces[i].Value;
                        double[] newComponents = new double[3];
                        for (int j = 0; j < 3; j++)
                        {
                            string work;
                            string newComponent = new Formula(components[j]).Calculate(out work, true);
                            if (work.Length > 0)
                            {
                                if (newComponent.Length > 0)
                                {
                                    work += $"\n{newComponent}";
                                }
                                if (!shownVec)
                                {
                                    workOutput += $"Vector steps: {ToString(pieces[i])}\n";
                                    parsed = true;
                                    shownVec = true;
                                }
                                workOutput += $"{components[j]}\n";
                                workOutput += $"{work}\n\n";
                            }
                            double num;
                            if (double.TryParse(newComponent, out num))
                            {
                                newComponents[j] = num;
                            }
                            else
                            {
                                workOutput += "Error: Unparsable vector component\n";
                                return "Error: Unparsable vector component";
                            }
                        }
                        pieces[i] = new Piece(new double3(newComponents[0], newComponents[1], newComponents[2]));
                    }
                }

                if (parsed)
                {
                    workOutput += "Steps:\n";
                }
            }

            // Convert from infix to postfix
            {
                string error;
                pieces = Infix2Postfix(pieces, out error);
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
                        if (firstLine)
                        {
                            firstLine = false;
                            workOutput += Postfix2Infix(stack, pieces, i);
                        }
                        else
                        {
                            workOutput += $"\n{Postfix2Infix(stack, pieces, i)}";
                        }

                        if ((stack.Count == 1 && piece.Value.Equals("-")) || (stack.Count >= 1 && piece.Value.Equals("neg")))
                        {
                            // Negate
                            Piece num = stack.Pop();
                            if (num.Type == "num")
                            {
                                stack.Push(new Piece(-(double)num.Value));
                            }
                            else if (num.Type == "vec")
                            {
                                stack.Push(new Piece(-(double3)num.Value));
                            }
                            else if (num.Type == "const")
                            {
                                stack.Push(new Piece(-num.ConstValue));
                            }
                            else
                            {
                                string error = $"Error: Cannot negate {ToString(num)}";
                                if (firstLine)
                                {
                                    firstLine = false;
                                    workOutput += error;
                                }
                                else
                                {
                                    workOutput += $"\n{error}";
                                }
                                return error;
                            }
                        }
                        else
                        {
                            if (stack.Count < 2)
                            {
                                string error = $"Error: Not enough operands for {piece.Value}";
                                if (firstLine)
                                {
                                    firstLine = false;
                                    workOutput += error;
                                }
                                else
                                {
                                    workOutput += $"\n{error}";
                                }
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
                                string error = $"Error: Cannot perform arithmetic on {num1.Value}";
                                if (firstLine)
                                {
                                    firstLine = false;
                                    workOutput += error;
                                }
                                else
                                {
                                    workOutput += $"\n{error}";
                                }
                                return error;
                            }
                            if (!isNum2 && num2.Type != "vec")
                            {
                                string error = $"Error: Cannot perform arithmetic on {num2.Value}";
                                if (firstLine)
                                {
                                    firstLine = false;
                                    workOutput += error;
                                }
                                else
                                {
                                    workOutput += $"\n{error}";
                                }
                                return error;
                            }

                            switch ((string)piece.Value)
                            {
                                case "+":
                                    if (isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece((double)num1.Value + (double)num2.Value));
                                    }
                                    else if (isNum1 && !isNum2)
                                    {
                                        stack.Push(new Piece((double)num1.Value + (double3)num2.Value));
                                    }
                                    else if (!isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece((double3)num1.Value + (double)num2.Value));
                                    }
                                    else
                                    {
                                        stack.Push(new Piece((double3)num1.Value + (double3)num2.Value));
                                    }
                                    break;
                                case "-":
                                    if (isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece((double)num1.Value - (double)num2.Value));
                                    }
                                    else if (isNum1 && !isNum2)
                                    {
                                        stack.Push(new Piece((double)num1.Value - (double3)num2.Value));
                                    }
                                    else if (!isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece((double3)num1.Value - (double)num2.Value));
                                    }
                                    else
                                    {
                                        stack.Push(new Piece((double3)num1.Value - (double3)num2.Value));
                                    }
                                    break;
                                case "*":
                                    if (isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece((double)num1.Value * (double)num2.Value));
                                    }
                                    else if (isNum1 && !isNum2)
                                    {
                                        stack.Push(new Piece((double)num1.Value * (double3)num2.Value));
                                    }
                                    else if (!isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece((double3)num1.Value * (double)num2.Value));
                                    }
                                    else
                                    {
                                        stack.Push(new Piece((double3)num1.Value * (double3)num2.Value));
                                    }
                                    break;
                                case "/":
                                    if (isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece((double)num1.Value / (double)num2.Value));
                                    }
                                    else if (isNum1 && !isNum2)
                                    {
                                        stack.Push(new Piece((double)num1.Value / (double3)num2.Value));
                                    }
                                    else if (!isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece((double3)num1.Value / (double)num2.Value));
                                    }
                                    else
                                    {
                                        stack.Push(new Piece((double3)num1.Value / (double3)num2.Value));
                                    }
                                    break;
                                case "x":
                                    if (!isNum1 && !isNum2)
                                    {
                                        stack.Push(new Piece(((double3)num1.Value).Cross((double3)num2.Value)));
                                    }
                                    else
                                    {
                                        string error = $"Error: Cannot get cross product of {ToString(num1)} and {ToString(num2)}";
                                        if (firstLine)
                                        {
                                            firstLine = false;
                                            workOutput += error;
                                        }
                                        else
                                        {
                                            workOutput += $"\n{error}";
                                        }
                                        return error;
                                    }
                                    break;
                                case ".":
                                    if (!isNum1 && !isNum2)
                                    {
                                        stack.Push(new Piece(((double3)num1.Value).Dot((double3)num2.Value)));
                                    }
                                    else
                                    {
                                        string error = $"Error: Cannot get dot product of {ToString(num1)} and {ToString(num2)}";
                                        if (firstLine)
                                        {
                                            firstLine = false;
                                            workOutput += error;
                                        }
                                        else
                                        {
                                            workOutput += $"\n{error}";
                                        }
                                        return error;
                                    }
                                    break;
                                case "%":
                                    if (isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece((double)num1.Value % (double)num2.Value));
                                    }
                                    else if (isNum1 && !isNum2)
                                    {
                                        stack.Push(new Piece((double)num1.Value % (double3)num2.Value));
                                    }
                                    else if (!isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece((double3)num1.Value % (double)num2.Value));
                                    }
                                    else
                                    {
                                        stack.Push(new Piece((double3)num1.Value % (double3)num2.Value));
                                    }
                                    break;
                                case "^":
                                    if (isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece(Math.Pow((double)num1.Value, (double)num2.Value)));
                                    }
                                    else if (isNum1 && !isNum2)
                                    {
                                        stack.Push(new Piece(Math2.Pow((double)num1.Value, (double3)num2.Value)));
                                    }
                                    else if (!isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece(Math2.Pow((double3)num1.Value, (double)num2.Value)));
                                    }
                                    else
                                    {
                                        stack.Push(new Piece(Math2.Pow((double3)num1.Value, (double3)num2.Value)));
                                    }
                                    break;
                            }
                        }
                    }
                    else if (piece.Type == "func1")
                    {
                        // Trig
                        if (firstLine)
                        {
                            firstLine = false;
                            workOutput += Postfix2Infix(stack, pieces, i);
                        }
                        else
                        {
                            workOutput += $"\n{Postfix2Infix(stack, pieces, i)}";
                        }

                        if (stack.Count == 0)
                        {
                            string error = $"Error: Not enough operands for {piece.Value}";
                            if (firstLine)
                            {
                                firstLine = false;
                                workOutput += error;
                            }
                            else
                            {
                                workOutput += $"\n{error}";
                            }
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
                            string error = $"Error: Cannot perform {piece} on {ToString(num)}";
                            if (firstLine)
                            {
                                firstLine = false;
                                workOutput += error;
                            }
                            else
                            {
                                workOutput += $"\n{error}";
                            }
                            return error;
                        }

                        switch ((string)piece.Value)
                        {
                            case "sin":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Math.Sin((double)num.Value * deg2rad)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Math2.Sin((double3)num.Value * deg2rad)));
                                }
                                break;
                            case "asin":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Math.Asin((double)num.Value) * rad2deg));
                                }
                                else
                                {
                                    stack.Push(new Piece(Math2.Asin((double3)num.Value) * rad2deg));
                                }
                                break;
                            case "cos":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Math.Cos((double)num.Value * deg2rad)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Math2.Cos((double3)num.Value * deg2rad)));
                                }
                                break;
                            case "acos":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Math.Acos((double)num.Value) * rad2deg));
                                }
                                else
                                {
                                    stack.Push(new Piece(Math2.Acos((double3)num.Value) * rad2deg));
                                }
                                break;
                            case "tan":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Math.Tan((double)num.Value * deg2rad)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Math2.Tan((double3)num.Value * deg2rad)));
                                }
                                break;
                            case "atan":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Math.Atan((double)num.Value) * rad2deg));
                                }
                                else
                                {
                                    stack.Push(new Piece(Math2.Atan((double3)num.Value) * rad2deg));
                                }
                                break;
                            case "abs":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Math.Abs((double)num.Value)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Math2.Abs((double3)num.Value)));
                                }
                                break;
                            case "floor":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Math.Floor((double)num.Value)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Math2.Floor((double3)num.Value)));
                                }
                                break;
                            case "ceil":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Math.Ceiling((double)num.Value)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Math2.Ceiling((double3)num.Value)));
                                }
                                break;
                            case "round":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Math.Round((double)num.Value)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Math2.Round((double3)num.Value)));
                                }
                                break;
                            case "sqrt":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Math.Sqrt((double)num.Value)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Math2.Sqrt((double3)num.Value)));
                                }
                                break;
                            case "deg":
                                if (isNum)
                                {
                                    stack.Push(new Piece((double)num.Value * rad2deg));
                                }
                                else
                                {
                                    stack.Push(new Piece((double3)num.Value * rad2deg));
                                }
                                break;
                            case "rad":
                                if (isNum)
                                {
                                    stack.Push(new Piece((double)num.Value * deg2rad));
                                }
                                else
                                {
                                    stack.Push(new Piece((double3)num.Value * deg2rad));
                                }
                                break;
                        }
                    }
                }
            }

            if (stack.Count > 1)
            {
                string error = "Error: Too many results";
                workOutput += $"\n{error}";
                return error;
            }
            
            if (stack.Count == 0)
            {
                string error = "Error: No results";
                workOutput += $"\n{error}";
                return error;
            }
            
            return AnswerToString(stack.Peek());
        }
    }
}
