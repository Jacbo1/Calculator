using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Calculator
{
    internal static class Matching
    {
        private const string UNSIGNED_NUMBER       = @"(\d*\.)?\d+";
        private const string NUMBER                = "[+-]?" + UNSIGNED_NUMBER;
        private const string SCI_NOTATION          = NUMBER + "E" + NUMBER;
        private const string UNSIGNED_SCI_NOTATION = UNSIGNED_NUMBER + "E" + NUMBER;
        private const string NUM_OR_SCI            = "(" + NUMBER + "|" + SCI_NOTATION + ")";
        private const string UNSIGNED_NUM_OR_SCI   = "(" + UNSIGNED_NUMBER + "|" + UNSIGNED_SCI_NOTATION + ")";
        private const string FRACTION              = NUM_OR_SCI + @" \/ " + NUM_OR_SCI;
        private const string UNSIGNED_FRACTION     = UNSIGNED_NUM_OR_SCI + @" \/ " + NUM_OR_SCI;
        private const string BINARY_NUMBER         = @"0b[01]+";
        private const string HEX_NUMBER            = @"0x[0-9A-Fa-f]+";
        private const string GENERIC_NUM           = "(" + HEX_NUMBER + "|" + BINARY_NUMBER + "|" + NUMBER + "|" + SCI_NOTATION + "|" + FRACTION + ")";
        private const string VECTOR                = "<" + GENERIC_NUM + ", ?" + GENERIC_NUM + ", ?" + GENERIC_NUM + ">";
        private const string VECTOR_LOOSE          = @"<[\S ]*?(, ?[\S ]*?)*>";
        private const string PIECE_REGEX_RIGHT     = VECTOR_LOOSE + "|" + HEX_NUMBER + "|" + BINARY_NUMBER + "|" + UNSIGNED_SCI_NOTATION + "|" + UNSIGNED_FRACTION + "|" + UNSIGNED_NUMBER + @"|[+%*/x.^()-])";

        private static readonly string[] defaultPieces = MergeSort(new string[] { @"\.x", @"\.y", @"\.z", @"band\(", @"bor\(", @"bnot\(", @"bxor\(", @"bshift\(", @"length\(", @"atan2\(", @"prod\(", @"getx\(", @"gety\(", @"getz\(", @"clamp\(", @"round\(", @"floor\(", @"norm\(", @"min\(", @"max\(", @"sum\(", @"avg\(", @"log\(", @"ceil\(", @"sign\(", @"sqrt\(", @"asin\(", @"acos\(", @"atan\(", @"sin\(", @"cos\(", @"tan\(", @"rad\(", @"deg\(", @"abs\(", @"avg\(", @"ln\(", "pi", "e" });

        private static readonly string PIECES      = "(" + string.Join("|", defaultPieces) + "|" + PIECE_REGEX_RIGHT;
        
        internal static readonly Regex
            RE_TrailingZeroes   = new Regex(@"(?<=\d+)(\.|(?<=\.\d+))0+($|[^\d])", RegexOptions.Compiled),
            RE_Number           = new Regex("^" + NUMBER + "$", RegexOptions.Compiled),
            RE_SciNotation      = new Regex("^(" + NUMBER + ")E(" + NUMBER + ")$", RegexOptions.Compiled),   // num1 = groups[1], num2 = groups[3]
            RE_Fraction         = new Regex("^" + FRACTION + "$", RegexOptions.Compiled),                    // num1 = groups[1], num2 = groups[5]
            RE_GenericNum       = new Regex("^" + GENERIC_NUM + "$", RegexOptions.Compiled),
            RE_Vector           = new Regex("^" + VECTOR + "$", RegexOptions.Compiled),                      // num1 = groups[1], num2 = groups[13], num3 = groups[25]]
            RE_VectorLoose      = new Regex(@"^<[\S ]*?(, ?[\S ]*?)*>$", RegexOptions.Compiled),
            RE_VectorFraction   = new Regex(@"^<(" + FRACTION + "), ?(" + FRACTION + "), ?(" + FRACTION + ")>$", RegexOptions.Compiled),  // num1 = groups[1], num2 = groups[10], num3 = groups[19]
            RE_Binary           = new Regex(@"(?<=^0b)[01]+$", RegexOptions.Compiled),
            RE_Hex              = new Regex(@"(?<=^0x)[0-9A-Fa-f]+$", RegexOptions.Compiled),
            RE_DefaultPieces    = new Regex(PIECES, RegexOptions.Compiled);

        internal static bool IsOperator(string s)
        {
            switch (s)
            {
                case "+":
                case "-":
                case "%":
                case "*":
                case "/":
                case ".":
                case "^":
                    return true;
            }
            return false;
        }

        internal static bool IsFunc(string s)
        {
            switch (s)
            {
                case "min(":
                case "max(":
                case "clamp(":
                case "log(":
                case "round(":
                case "sum(":
                case "prod(":
                case "getx(":
                case "gety(":
                case "getz(":
                case "length(":
                case "norm(":
                case "atan2(":
                case "ln(":
                case "band(":
                case "bor(":
                case "bxor(":
                case "bshift(":
                case "bnot(":
                case "avg(":
                case "ceil(":
                case "floor(":
                case "sign(":
                case "sqrt(":
                case "asin(":
                case "acos(":
                case "atan(":
                case "sin(":
                case "cos(":
                case "tan(":
                case "rad(":
                case "deg(":
                case "abs(":
                    return true;
            }
            return false;
        }



        // Construct the regex for matching pieces in strings
        internal static string ConstructPieceRegex(Array newPieces)
        {
            string[] pieces;
            if (newPieces.Length > 0)
            {
                // Add and sort largest to smallest
                pieces = new string[defaultPieces.Length + newPieces.Length];
                Array.Copy(defaultPieces, pieces, defaultPieces.Length);
                Array.Copy(newPieces, 0, pieces, defaultPieces.Length, newPieces.Length);
                MergeSort(pieces, 0, pieces.Length - 1);
            }
            else
            {
                // Default pieces are already sorted
                pieces = defaultPieces;
            }

            return "(" + string.Join("|", pieces) + "|" + PIECE_REGEX_RIGHT;
        }

        internal static int FindClosingParenthesis(List<Piece> pieces, int start)
        {
            int openCount = 0;
            for (int i = start; i < pieces.Count; i++)
            {
                Piece piece2 = pieces[i];
                if (piece2.Value.Equals("("))
                    openCount++;
                else if (piece2.Value.Equals(")"))
                {
                    openCount--;
                    if (openCount == 0)
                        return i; // Found partner parenthesis
                }
            }
            return -1;
        }

        internal static int FindClosingParenthesis(string raw, int start)
        {
            int openCount = 0;
            int open = raw.IndexOf('(', start);
            int close = raw.IndexOf(')', start);
            int index;

            if (open == -1 || close == -1)
                return -1;

            if (open < close)
            {
                index = open;
                openCount++;
            }
            else
                return -1;

            for(; ; )
            {
                if (open != -1 && index >= open)
                    open = raw.IndexOf('(', index + 1);
                if (index >= close)
                {
                    close = raw.IndexOf(')', index + 1);
                    if (close == -1)
                        return -1;
                }
                if (close < open || open == -1)
                {
                    // Closing
                    index = close;
                    openCount--;
                    if (openCount <= 0)
                        return index;
                }
                else
                    openCount++; // Opening
            }
        }

        internal static int FindOpeningParenthesis(List<Piece> pieces, int start)
        {
            int openCount = 0;
            for (int i = start; i >= 0; i--)
            {
                Piece piece2 = pieces[i];
                if (piece2.Value.Equals(")"))
                    openCount++;
                else if (piece2.Value.Equals("("))
                {
                    openCount--;
                    if (openCount == 0)
                        return i; // Found partner parenthesis
                }
            }
            return -1;
        }

        // Helper functions
        private static void Merge(string[] arr, int l, int m, int r)
        {
            string[] leftArr = new string[m - l + 1];
            string[] rightArr = new string[r - m];

            Array.Copy(arr, l, leftArr, 0, m - l + 1);
            Array.Copy(arr, m + 1, rightArr, 0, r - m);

            int i = 0;
            int j = 0;
            for (int k = l; k < r + 1; k++)
            {
                if (i == leftArr.Length)
                {
                    arr[k] = rightArr[j];
                    j++;
                }
                else if (j == rightArr.Length)
                {
                    arr[k] = leftArr[i];
                    i++;
                }
                else if (leftArr[i].Length >= rightArr[j].Length)
                {
                    arr[k] = leftArr[i];
                    i++;
                }
                else
                {
                    arr[k] = rightArr[j];
                    j++;
                }
            }
        }

        private static void MergeSort(string[] arr, int l, int r)
        {
            if (l < r)
            {
                int m = (l + r) / 2;
                MergeSort(arr, l, m);
                MergeSort(arr, m + 1, r);
                Merge(arr, l, m, r);
            }
        }

        private static string[] MergeSort(string[] arr)
        {
            MergeSort(arr, 0, arr.Length - 1);
            return arr;
        }

        internal static string Answer2Decimal(string answer)
        {
            if (RE_Fraction.IsMatch(answer) && Fraction.TryParse(answer, out Fraction? frac))
                return frac.ToString();

            {
                Match match = RE_VectorFraction.Match(answer);
                if (match.Success)
                {
                    if (!Fraction.TryParse(match.Groups[1].Value, out Fraction? x)) return answer;
                    if (!Fraction.TryParse(match.Groups[10].Value, out Fraction? y)) return answer;
                    if (!Fraction.TryParse(match.Groups[19].Value, out Fraction? z)) return answer;
                    return $"<{x}, {y}, {z}>";
                }
            }

            return answer;
        }
    }
}
