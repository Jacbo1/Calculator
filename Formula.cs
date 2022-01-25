using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Calculator
{
    internal class Formula
    {
        private const double deg2rad = Math.PI / 180;
        private const double rad2deg = 180 / Math.PI;
        private const int decDigitDisplay = 10;
        private const double decSciThreshold = 0.00001;

        // Incredibly easy and simple method to find the pieces and put them into a list in order and allows less strict input formatting
        private static Regex pieceRegex;
        private static Regex trailingZeroes = new Regex(@"(?<=[0-9]+)(\.|(?<=\.[0-9]+))0+($|[^0-9])", RegexOptions.Compiled);

        private Dictionary<string, Piece> vars;

        public string formula = "";

        public Formula (string formula)
        {
            this.formula = formula.Replace(" ", "").Replace("\t", "");
            vars = new Dictionary<string, Piece>();
            ConstructPieceRegex();
        }

        public Formula (string formula, Dictionary<string, Piece> vars)
        {
            this.formula = formula.Replace(" ", "").Replace("\t", "");
            this.vars = vars;
            ConstructPieceRegex();
        }

        private static void Merge(string[] arr, int l, int m, int r)
        {
            int n1 = m - l + 1;
            int n2 = r - m;

            string[] L = new string[n1];
            string[] R = new string[n2];

            for (int i = 0; i < n1; i++)
            {
                L[i] = arr[l + i];
                for (int j = 0; j < n2; j++)
                {
                    R[j] = arr[m + 1 + j];

                    int k = l;
                    int a = 0;
                    int b = 0;
                    while (a < n1 && b < n2)
                    {
                        if (L[a].Length > R[b].Length)
                        {
                            arr[k] = L[a];
                            a++;
                        }
                        else
                        {
                            arr[k] = R[b];
                            b++;
                        }
                        k++;
                    }

                    while (a < n1)
                    {
                        arr[k] = L[a];
                        a++;
                        k++;
                    }

                    while (b < n1)
                    {
                        arr[k] = R[b];
                        b++;
                        k++;
                    }
                }
            }
        }

        private static void MergeSort(string[] arr, int l, int r)
        {
            if (l < r)
            {
                int m = l + (r - 1) / 2;
                MergeSort(arr, l, m);
                MergeSort(arr, m + 1, r);
                Merge(arr, l, m, r);
            }
        }

        private void ConstructPieceRegex()
        {
            string regex = "(";
            string[] keys = vars.Keys.ToArray();
            MergeSort(keys, 0, keys.Length - 1);
            for (int i = 0; i < keys.Length; i++)
            {
                regex += $"{keys[i]}|";
            }
            //                        |    scientific notation e.g. -1.23E10    | |vars ||    vector    | +  - *  /  ^  % sin asin cos acos tan atan rad deg abs floor ceil round sqrt (  )  |     number    | e pi x .
            pieceRegex = new Regex($@"(-?([0-9]*\.)?[0-9]+)E(-?([0-9]*\.)?[0-9]+)|{regex}<\S+?,\S+?,\S+?>|\+|-|\*|\/|\^|%|sin|asin|cos|acos|tan|atan|rad|deg|abs|floor|ceil|round|sqrt|\(|\)|([0-9]*\.)?[0-9]+|e|pi|x|\.)", RegexOptions.Multiline);
        }

        public void SetVar(string key, Piece value)
        {
            vars[key] = value;
            ConstructPieceRegex();
        }

        private static double Abs(double n) => n < 0 ? -n : n;

        private static string ToString(double n)
        {
            if (n == 0)
            {
                return "0";
            }
            if (Abs(n) < decSciThreshold)
            {
                double digits = Math.Floor(Math.Log10(Abs(n)));
                double sci = Math.Round(n * Math.Pow(10, -digits), decDigitDisplay);
                string num = trailingZeroes.Replace(sci.ToString(), "");
                if (num.IndexOf('.') == -1)
                {
                    num += ".0";
                }
                return $"{num}E{(int)digits}";
            }
            double rounded = Math.Round(n, decDigitDisplay);
            return trailingZeroes.Replace(rounded.ToString(), "");
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
                    return ToString((double)piece.Value);
                case "vec":
                    Vector num = (Vector)piece.Value;
                    return $"<{ToString(num.X)}, {ToString(num.Y)}, {ToString(num.Z)}>";
                case "parse vec":
                    string[] components = (string[])piece.Value;
                    return $"<{components[0]}, {components[1]}, {components[2]}>";

            }
        }

        private static string AnswerToString(Piece piece, bool round)
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
                    return round ? ToString((double)piece.Value) : ((double)piece.Value).ToString();
                case "const":
                    return round ? ToString(piece.ConstValue) : piece.ConstValue.ToString();
                case "vec":
                    Vector num = (Vector)piece.Value;
                    string x = round ? ToString(num.X) : num.X.ToString();
                    string y = round ? ToString(num.Y) : num.Y.ToString();
                    string z = round ? ToString(num.Z) : num.Z.ToString();
                    return $"<{x}, {y}, {z}>";
                case "parse vec":
                    string[] components = (string[])piece.Value;
                    return $"<{components[0]}, {components[1]}, {components[2]}>";

            }
        }

        private static int FindClosingParenthesis(List<Piece> pieces, int start)
        {
            int openCount = 0;
            for (int i = start; i < pieces.Count; i++)
            {
                Piece piece2 = pieces[i];
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
                        return i;
                    }
                }
            }
            return -1;
        }

        private static int FindOpeningParenthesis(List<Piece> pieces, int start)
        {
            int openCount = 0;
            for (int i = start; i >= 0; i--)
            {
                Piece piece2 = pieces[i];
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
                        return i;
                    }
                }
            }
            return -1;
        }

        private List<Piece> String2Infix(string formula)
        {
            List<Piece> pieces = new List<Piece>();
            foreach (Match match in pieceRegex.Matches(formula))
            {
                string value = match.Value;
                if (vars.ContainsKey(value))
                {
                    pieces.Add(vars[value]);
                }
                else
                {
                    pieces.Add(new Piece(match.Value));
                }
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
                            pieces[i] = new Piece(-(double)pieces[i + 1].Value);
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
                    piece.Type == "const"))
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
                                    int temp = FindClosingParenthesis(pieces, i + 2);
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
                                        else if(piece2.Type == "func1")
                                        {
                                            lastWasFunc1 = true;
                                        }
                                        else if (piece2.Value.Equals("("))
                                        {
                                            // Find closing parenthesis
                                            int temp = FindClosingParenthesis(pieces, close);
                                            if (temp != -1)
                                            {
                                                close = temp;
                                            }
                                        }
                                        close++;
                                    }
                                }
                            }
                            else if(nextPiece.Value.Equals("("))
                            {
                                // Not a func1 to the right
                                // Parentheses to the right
                                // Find close pos
                                int temp = FindClosingParenthesis(pieces, i + 1);
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
                                    int temp = FindOpeningParenthesis(pieces, i);
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
            // Remove unnecessary parentheses
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
                        else if (openCount == 1 && piece2.Precedence != -1 )
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
                if (piece.IsOperand)
                {
                    // Operand
                    // Push to output
                    postfix.Add(piece);
                }
                else if(piece.Type == "func1")
                {
                    // Func1
                    // Push to stack
                    stack.Push(piece);
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

        public string Calculate(out string workOutput)
        {
            return Calculate(out workOutput, false, false);
        }

        public string Calculate(out string workOutput, bool isSub, bool round)
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
                            string newComponent = new Formula(components[j], vars).Calculate(out work, true, false);
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
                        pieces[i] = new Piece(new Vector(newComponents[0], newComponents[1], newComponents[2]));
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

                        if (stack.Count == 1 && piece.Value.Equals("-"))
                        {
                            // Negate
                            Piece num = stack.Pop();
                            if (num.Type == "num")
                            {
                                stack.Push(new Piece(-(double)num.Value));
                            }
                            else if (num.Type == "vec")
                            {
                                stack.Push(new Piece(-(Vector)num.Value));
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
                                string error = $"Error: Not enough operands for {ToString(piece)}";
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
                                string error = $"Error: Cannot perform arithmetic on {ToString(num1)}";
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
                                string error = $"Error: Cannot perform arithmetic on {ToString(num2)}";
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
                                        stack.Push(new Piece((double)num1.Value + (Vector)num2.Value));
                                    }
                                    else if (!isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece((Vector)num1.Value + (double)num2.Value));
                                    }
                                    else
                                    {
                                        stack.Push(new Piece((Vector)num1.Value + (Vector)num2.Value));
                                    }
                                    break;
                                case "-":
                                    if (isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece((double)num1.Value - (double)num2.Value));
                                    }
                                    else if (isNum1 && !isNum2)
                                    {
                                        stack.Push(new Piece((double)num1.Value - (Vector)num2.Value));
                                    }
                                    else if (!isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece((Vector)num1.Value - (double)num2.Value));
                                    }
                                    else
                                    {
                                        stack.Push(new Piece((Vector)num1.Value - (Vector)num2.Value));
                                    }
                                    break;
                                case "*":
                                    if (isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece((double)num1.Value * (double)num2.Value));
                                    }
                                    else if (isNum1 && !isNum2)
                                    {
                                        stack.Push(new Piece((double)num1.Value * (Vector)num2.Value));
                                    }
                                    else if (!isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece((Vector)num1.Value * (double)num2.Value));
                                    }
                                    else
                                    {
                                        stack.Push(new Piece((Vector)num1.Value * (Vector)num2.Value));
                                    }
                                    break;
                                case "/":
                                    if (isNum2)
                                    {
                                        if ((double)num2.Value == 0)
                                        {
                                            string error = $"Error: Division by 0";
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
                                        Vector vec = (Vector)num2.Value;
                                        if (vec.X == 0 || vec.Y == 0 || vec.Z == 0)
                                        {
                                            string error = $"Error: Division by 0";
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
                                    if (isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece((double)num1.Value / (double)num2.Value));
                                    }
                                    else if (isNum1 && !isNum2)
                                    {
                                        stack.Push(new Piece((double)num1.Value / (Vector)num2.Value));
                                    }
                                    else if (!isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece((Vector)num1.Value / (double)num2.Value));
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
                                        stack.Push(new Piece(((Vector)num1.Value).Dot((Vector)num2.Value)));
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
                                        stack.Push(new Piece((double)num1.Value % (Vector)num2.Value));
                                    }
                                    else if (!isNum1 && isNum2)
                                    {
                                        stack.Push(new Piece((Vector)num1.Value % (double)num2.Value));
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
                                            stack.Push(new Piece(Math.Pow((double)num1.Value, (double)num2.Value)));
                                        }
                                        else if (isNum1 && !isNum2)
                                        {
                                            stack.Push(new Piece(Vector.Pow((double)num1.Value, (Vector)num2.Value)));
                                        }
                                        else if (!isNum1 && isNum2)
                                        {
                                            stack.Push(new Piece(Vector.Pow((Vector)num1.Value, (double)num2.Value)));
                                        }
                                        else
                                        {
                                            stack.Push(new Piece(Vector.Pow((Vector)num1.Value, (Vector)num2.Value)));
                                        }
                                    }
                                    catch
                                    {
                                        string error = $"Error: Cannot raise {ToString(num1)} to the {ToString(num2)} power";
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
                            }
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
                            workOutput += $"\n{Postfix2Infix(stack, pieces, i)}";
                        }

                        if (stack.Count == 0)
                        {
                            string error = $"Error: Not enough operands for {ToString(piece)}";
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
                            string error = $"Error: Cannot perform {ToString(piece)} on {ToString(num)}";
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
                                    stack.Push(new Piece(Vector.Sin((Vector)num.Value * deg2rad)));
                                }
                                break;
                            case "asin":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Math.Asin((double)num.Value) * rad2deg));
                                }
                                else
                                {
                                    stack.Push(new Piece(Vector.Asin((Vector)num.Value) * rad2deg));
                                }
                                break;
                            case "cos":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Math.Cos((double)num.Value * deg2rad)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Vector.Cos((Vector)num.Value * deg2rad)));
                                }
                                break;
                            case "acos":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Math.Acos((double)num.Value) * rad2deg));
                                }
                                else
                                {
                                    stack.Push(new Piece(Vector.Acos((Vector)num.Value) * rad2deg));
                                }
                                break;
                            case "tan":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Math.Tan((double)num.Value * deg2rad)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Vector.Tan((Vector)num.Value * deg2rad)));
                                }
                                break;
                            case "atan":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Math.Atan((double)num.Value) * rad2deg));
                                }
                                else
                                {
                                    stack.Push(new Piece(Vector.Atan((Vector)num.Value) * rad2deg));
                                }
                                break;
                            case "abs":
                                if (isNum)
                                {
                                    double temp = (double)num.Value;
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
                                    stack.Push(new Piece(Math.Floor((double)num.Value)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Vector.Floor((Vector)num.Value)));
                                }
                                break;
                            case "ceil":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Math.Ceiling((double)num.Value)));
                                }
                                else
                                {
                                    stack.Push(new Piece(Vector.Ceiling((Vector)num.Value)));
                                }
                                break;
                            case "round":
                                if (isNum)
                                {
                                    stack.Push(new Piece(Math.Round((double)num.Value)));
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
                                        stack.Push(new Piece(Math.Pow((double)num.Value, 0.5)));
                                    }
                                    else
                                    {
                                        stack.Push(new Piece(Vector.Sqrt((Vector)num.Value)));
                                    }
                                }
                                catch
                                {
                                    string error = $"Error: Cannot square root {num}";
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
                            case "deg":
                                if (isNum)
                                {
                                    stack.Push(new Piece((double)num.Value * rad2deg));
                                }
                                else
                                {
                                    stack.Push(new Piece((Vector)num.Value * rad2deg));
                                }
                                break;
                            case "rad":
                                if (isNum)
                                {
                                    stack.Push(new Piece((double)num.Value * deg2rad));
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
                                    stack.Push(new Piece(-(double)num.Value));
                                }
                                else
                                {
                                    stack.Push(new Piece(-(Vector)num.Value));
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

            return AnswerToString(stack.Peek(), round);
        }
    }
}
