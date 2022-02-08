using System;

namespace Calculator
{
    internal class FunctionException : Exception
    {
        public string FunctionName = "";

        public FunctionException() : base() { }

        public FunctionException(string message) : base(message) { }

        public FunctionException(string funcName, string message) : base(message)
        {
            FunctionName = funcName;
        }

        public FunctionException(string message, Exception inner) : base(message, inner) { }
    }
}
