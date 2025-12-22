using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Exceptions
{
    internal class AccessDbException: Exception
    {
        private readonly string details;

        public AccessDbException(string cause, string details) : base(cause)
        {
            this.details = details;
        }

        public override string ToString()
        {
            return $"""
                Cause: 
                ========
                {details}
                Details: 
                ========
                {this.Message}
                Exception: 
                ======
                {this.StackTrace}
                """;
        }
    }
}
