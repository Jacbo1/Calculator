using System.Numerics;
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
                    IsOperand = false;
                    break;
            }
        }

        public Piece(Fraction? constValue) : this()
        {
            ConstValue = constValue;
        }
    }
}
