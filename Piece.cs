﻿using NewMath;
using System;
using System.Text.RegularExpressions;

namespace Calculator
{
    internal class Piece
    {
        private static Regex vectorRegex = new Regex(@"^<(\S+?),(\S+?),(\S+?)>$");
        private static Regex strictVectorRegex = new Regex(@"^<(-?([0-9]*\.)?[0-9]+),(-?([0-9]*\.)?[0-9]+),(-?([0-9]*\.)?[0-9]+)>$");
        private static Regex numberRegex = new Regex(@"^-?([0-9]*\.)?[0-9]+$");

        public string Type;
        public object Value;
        public int Precedence = -1;
        public double ConstValue;
        public bool IsOperand = false;

        public Piece(double num)
        {
            Type = "num";
            Value = num;
            IsOperand = true;
        }

        public Piece(double3 vec)
        {
            Type = "vec";
            Value = vec;
            IsOperand = true;
        }

        public Piece(string piece)
        {
            switch (piece)
            {
                case "e":
                    Type = "const";
                    Value = "e";
                    ConstValue = Math.E;
                    IsOperand = true;
                    break;
                case "pi":
                    Type = "const";
                    Value = "pi";
                    ConstValue = Math.PI;
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
                    Type = "trig";
                    Value = "sin";
                    Precedence = 7;
                    break;
                case "asin":
                    Type = "trig";
                    Value = "asin";
                    Precedence = 7;
                    break;
                case "cos":
                    Type = "trig";
                    Value = "cos";
                    Precedence = 7;
                    break;
                case "acos":
                    Type = "trig";
                    Value = "acos";
                    Precedence = 7;
                    break;
                case "tan":
                    Type = "trig";
                    Value = "tan";
                    Precedence = 7;
                    break;
                case "atan":
                    Type = "trig";
                    Value = "atan";
                    Precedence = 7;
                    break;
                case "deg":
                    Type = "trig";
                    Value = "deg";
                    Precedence = 7;
                    break;
                case "rad":
                    Type = "trig";
                    Value = "rad";
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
                    Type = "op";
                    Value = "neg";
                    Precedence = 5;
                    break;
                default:
                    // Check if it is a vector
                    Match match = vectorRegex.Match(piece);
                    if (match.Success)
                    {
                        Match vec = strictVectorRegex.Match(piece);
                        if (vec.Success)
                        {
                            // Valid vector
                            Type = "vec";
                            Value = new double3(
                                double.Parse(vec.Groups[1].Value),
                                double.Parse(vec.Groups[3].Value),
                                double.Parse(vec.Groups[5].Value));
                        }
                        else
                        {
                            // Vector that needs parsing
                            Type = "parse vec";
                            Value = new string[] {
                                match.Groups[1].Value,
                                match.Groups[2].Value,
                                match.Groups[3].Value
                            };
                        }
                        IsOperand = true;
                        return;
                    }

                    // Check if it is a number
                    match = numberRegex.Match(piece);
                    if (match.Success)
                    {
                        // Valid number
                        Type = "num";
                        Value = double.Parse(match.Value);
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