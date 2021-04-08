using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Controls.Auto.Forms.Validation
{
    public class AutoFormsValidationAttribute : Attribute
    {
        public enum ValidationType
        {
            MinLength,
            MaxLength,
            Required,
            Email,
            Numeric,
            Unknown
        }

        public ValidationType Type { get; private set; }
        public AutoFormsValidationAttribute(ValidationType type = ValidationType.Unknown)
        {
            Type = type;
        }

        public virtual bool IsValid(object obj) { return true; }
    }
}
