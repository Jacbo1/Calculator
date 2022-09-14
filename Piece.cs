using System;
using System.Text.RegularExpressions;

namespace Calculator
{
    internal class Piece
    {
        public string Type;
        public object Value;
        public int Precedence = -1;
        public Fraction ConstValue;
        public bool IsOperand = false;
        public bool IsVar = false;
        public string VarName;

        public Piece(Fraction num)
        {
            Type = "num";
            Value = num;
            IsOperand = true;
        }

        public Piece(Vector vec)
        {
            Type = "vec";
            Value = vec;
            IsOperand = true;
        }

        public Piece(Function func)
        {
            Type = "func";
            Value = func;
            //IsOperand = true;
            Precedence = 8;
        }

        public Piece(string piece)
        {
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
                case "sigfig4":
                case "sign":
                case "sin":
                case "asin":
                case "cos":
                case "acos":
                case "tan":
                case "atan":
                case "deg":
                case "rad":
                case "abs":
                case "floor":
                case "round":
                case "sqrt":
                    Type = "func1";
                    Value = piece;
                    Precedence = 7;
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
                    if (Fraction.TryParse(piece, out Fraction frac))
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
                            Fraction.Parse(match.Groups[1].Value),
                            Fraction.Parse(match.Groups[13].Value),
                            Fraction.Parse(match.Groups[25].Value));
                        IsOperand = true;
                        return;
                    }

                    // Check if it is an unprocessed vector
                    match = Matching.RE_VectorLoose.Match(piece);
                    if (match.Success)
                    {
                        // Vector that needs parsing
                        Type = "parse vec";
                        Value = new string[] {
                                match.Groups[1].Value,
                                match.Groups[2].Value,
                                match.Groups[3].Value
                            };
                        IsOperand = true;
                        return;
                    }

                    // No match
                    Type = "error";
                    Value = piece;
                    break;
            }
        }
    }
}
