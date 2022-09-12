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
                case "sigfig4":
                    Type = "func1";
                    Value = "sigfig4";
                    Precedence = 7;
                    break;
                case "sign":
                    Type = "func1";
                    Value = "sign";
                    Precedence = 7;
                    break;
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
                case "sin":
                    Type = "func1";
                    Value = "sin";
                    Precedence = 7;
                    break;
                case "asin":
                    Type = "func1";
                    Value = "asin";
                    Precedence = 7;
                    break;
                case "cos":
                    Type = "func1";
                    Value = "cos";
                    Precedence = 7;
                    break;
                case "acos":
                    Type = "func1";
                    Value = "acos";
                    Precedence = 7;
                    break;
                case "tan":
                    Type = "func1";
                    Value = "tan";
                    Precedence = 7;
                    break;
                case "atan":
                    Type = "func1";
                    Value = "atan";
                    Precedence = 7;
                    break;
                case "deg":
                    Type = "func1";
                    Value = "deg";
                    Precedence = 7;
                    break;
                case "rad":
                    Type = "func1";
                    Value = "rad";
                    Precedence = 7;
                    break;
                case "abs":
                    Type = "func1";
                    Value = "abs";
                    Precedence = 7;
                    break;
                case "floor":
                    Type = "func1";
                    Value = "floor";
                    Precedence = 7;
                    break;
                case "ceil":
                    Type = "func1";
                    Value = "ceil";
                    Precedence = 7;
                    break;
                case "round":
                    Type = "func1";
                    Value = "round";
                    Precedence = 7;
                    break;
                case "sqrt":
                    Type = "func1";
                    Value = "sqrt";
                    Precedence = 7;
                    break;
                case "+":
                    Type = "op";
                    Value = "+";
                    Precedence = 1;
                    break;
                case "-":
                    Type = "op";
                    Value = "-";
                    Precedence = 1;
                    break;
                case "*":
                    Type = "op";
                    Value = "*";
                    Precedence = 2;
                    break;
                case "/":
                    Type = "op";
                    Value = "/";
                    Precedence = 2;
                    break;
                case "x":
                    Type = "op";
                    Value = "x";
                    Precedence = 2;
                    break;
                case ".":
                    Type = "op";
                    Value = ".";
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
