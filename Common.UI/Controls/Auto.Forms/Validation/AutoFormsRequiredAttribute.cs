using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Controls.Auto.Forms.Validation
{
    public class AutoFormsRequiredAttribute : AutoFormsValidationAttribute
    {
        public AutoFormsRequiredAttribute():base(ValidationType.Required) 
        {
        }

        public override bool IsValid(object obj)
        {
            if (obj == null)
                return false;

            var s = obj as string;
            if (s != null && string.IsNullOrWhiteSpace(s))
                return false;

            return true;
        }
    }

}
