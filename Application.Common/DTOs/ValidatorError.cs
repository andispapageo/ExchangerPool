using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.DTOs
{
    public class ValidationError
    {
        public string Identifier { get; set; }

        public string ErrorMessage { get; set; }

        public string ErrorCode { get; set; }

        public ValidationSeverity Severity { get; set; }

        public ValidationError()
        {
        }

        public ValidationError(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public ValidationError(string identifier, string errorMessage)
        {
            Identifier = identifier;
            ErrorMessage = errorMessage;
        }

        public ValidationError(string identifier, string errorMessage, string errorCode, ValidationSeverity severity)
        {
            Identifier = identifier;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
            Severity = severity;
        }
    }

    public enum ValidationSeverity
    {
        Error,
        Warning,
        Info
    }
}
