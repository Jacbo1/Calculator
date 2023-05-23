using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Calculator
{
    internal struct Piece
    {
        public string Type;
        public object Value;
        public int Precedence;
        public Fraction? ConstValue;
        public bool IsOperand;
        public bool IsVar;
        public string VarName;

        public Piece(Fraction num)
        {
            Precedence = -1;
            IsVar = false;
            ConstValue = null;
            VarName = null;

            Type = "num";
            Value = num;
            IsOperand = true;
        }

        public Piece(Vector vec)
        {
            Precedence = -1;
            IsVar = false;
            ConstValue = null;
            VarName = null;

            Type = "vec";
            Value = vec;
            IsOperand = true;
        }

        public Piece(Function func)
        {
            IsVar = false;
            ConstValue = null;
            VarName = null;

            Type = "func";
            Value = func;
            IsOperand = false;
            Precedence = 8;
        }

        public Piece(string piece)
        {
            IsVar = false;
            ConstValue = null;
            IsOperand = false;
            Precedence = -1;
            VarName = null;

            switch (piece)
            {
                case "e":
                    Type = "const";
                    Value = "e";
                    ConstValue = Fraction.E;
                    IsOperand = true;
                    break;
                case "pi":
                    Type = "const";
                    Value = "pi";
                    ConstValue = Fraction.PI;
                    IsOperand = true;
                    break;
                case "(":
                    Type = "lpar";
                    Value = "(";
                    break;
                case ")":
                    Type = "rpar";
                    Value = ")";
                    break;
                case "+":
                case "-":
                    Type = "op";
                    Value = piece;
                    Precedence = 1;
                    break;
                case "*":
                case "/":
                case "x":
                case ".":
                    Type = "op";
                    Value = piece;
                    Precedence = 2;
                    break;
                case "%":
                    Type = "op";
                    Value = "%";
                    Precedence = 3;
                    break;
                case "^":
                    Type = "op";
                    Value = "^";
                    Precedence = 6;
                    break;
                case "neg":
                    Type = "func1";
                    Value = "neg";
                    Precedence = 5;
                    break;
                default:
                    // Check if it is a hex/binary/number
                    if (Fraction.TryParse(piece, out Fraction? frac))
                    {
                        Type = "num";
                        Value = frac;
                        IsOperand = true;
                        return;
                    }

                    // Check if it is a vector
                    Match match = Matching.RE_Vector.Match(piece);
                    if (match.Success)
                    {
                        Type = "vec";
                        Value = new Vector(
                            (Fraction)Fraction.Parse(match.Groups[1].Value),
                            (Fraction)Fraction.Parse(match.Groups[13].Value),
                            (Fraction)Fraction.Parse(match.Groups[25].Value));
                        IsOperand = true;
                        return;
                    }

                    // Check if it is an unprocessed vector
                    if (Matching.RE_VectorLoose.Match(piece).Success)
                    {
                        // Vector that needs parsing
                        Type = "parse vec";
                        int last = 1;
                        int stop = FindNextVectorPartition(piece, 1);
                        string x = piece.Substring(1, stop - 1);
                        Console.WriteLine(stop);
                        last = stop + 1;
                        if (last < piece.Length)
                        {
                            stop = FindNextVectorPartition(piece, last);
                            string y = piece.Substring(last, stop - last);
                            last = stop + 1;
                            if (last < piece.Length)
                            {
                                stop = FindNextVectorPartition(piece, last);
                                string z = piece.Substring(last, stop - last);
                                Value = new string[] { x, y, z};
                            }
                            else Value = new string[] { x, y, "0" };
                        }
                        else Value = new string[] { x, x, x };

                        IsOperand = true;
                        return;
                    }

                    // No match
                    Type = "error";
                    Value = piece;
                    IsOperand = false;
                    break;
            }
        }

        private static int FindNextVectorPartition(string input, int start)
        {
            int openCount = 0;
            for (int i = start; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case '(': openCount++; break;
                    case ')': openCount--; break;
                    case ',':
                    case '>':
                        if (openCount <= 0) return i;
                        break;
                }
            }

            return input.Length - 1;
        }
    }
}
