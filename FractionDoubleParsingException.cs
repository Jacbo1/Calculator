using System;

namespace Calculator
{
    public class FractionDoubleParsingException : Exception
    {
        public FractionDoubleParsingException() : base("Number too small or large for Fraction.") { }

        public FractionDoubleParsingException(string message) : base(message) { }

        public FractionDoubleParsingException(string message, Exception inner) : base(message, inner) { }
    }
}
