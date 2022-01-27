using System.Text.RegularExpressions;

namespace Calculator
{
    internal static class Regexes
    {
        private const string  NUMBER        = "[+-]?(\\d*\\.)?\\d+";
        private const string  UNSIGNED_NUMBER = "(\\d*\\.)?\\d+";
        private const string  SCI_NOTATION  = NUMBER + "E" + NUMBER;
        private const string  UNSIGNED_SCI_NOTATION  = UNSIGNED_NUMBER + "E" + NUMBER;
        private const string  NUM_OR_SCI    = "(" + NUMBER + "|" + SCI_NOTATION + ")";
        private const string  UNSIGNED_NUM_OR_SCI    = "(" + UNSIGNED_NUMBER + "|" + UNSIGNED_SCI_NOTATION + ")";
        private const string  FRACTION      = NUM_OR_SCI + " \\/ " + NUM_OR_SCI;
        private const string  UNSIGNED_FRACTION      = UNSIGNED_NUM_OR_SCI + " \\/ " + NUM_OR_SCI;
        private const string  GENERIC_NUM   = "(" + NUMBER + "|" + SCI_NOTATION + "|" + FRACTION + ")";
        private const string  VECTOR        = "<" + GENERIC_NUM + ", ?" + GENERIC_NUM + ", ?" + GENERIC_NUM + ">";
        private const string  VECTOR_LOOSE  = "<[\\S ]*?, ?[\\S ]*?, ?[\\S ]*?>";
        internal const string PIECES_RIGHT  = VECTOR_LOOSE + "|" + UNSIGNED_FRACTION + "|" + UNSIGNED_SCI_NOTATION + "|" + UNSIGNED_NUMBER + "|asin|sin|acos|cos|atan|tan|rad|deg|abs|floor|ceil|round|sqrt|e|pi|[+%*/x.^()-])";
        internal static readonly Regex
            RE_TrailingZeroes = new Regex(@"(?<=\d+)(\.|(?<=\.\d+))0+($|[^\d])", RegexOptions.Compiled),
            RE_Number = new Regex("^" + NUMBER + "$", RegexOptions.Compiled),
            RE_SciNotation = new Regex("^(" + NUMBER + ")E(" + NUMBER + ")$", RegexOptions.Compiled),   // num1 = groups[1], num2 = groups[3]
            RE_Fraction = new Regex("^" + FRACTION + "$", RegexOptions.Compiled),                       // num1 = groups[1], num2 = groups[5]
            RE_GenericNum = new Regex("^" + GENERIC_NUM + "$", RegexOptions.Compiled),
            RE_Vector = new Regex("^" + VECTOR + "$", RegexOptions.Compiled),                           // num1 = groups[1], num2 = groups[13], num3 = groups[25]
            RE_VectorLoose = new Regex(@"^<([\S ]*?), ?([\S ]*?), ?([\S ]*?)>$", RegexOptions.Compiled);   // num1 = groups[1], num2 = groups[2],  num3 = groups[3]
    }
}
