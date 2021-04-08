using System;
using System.Text.RegularExpressions;

namespace Common.UI.Controls.Auto.Forms.Validation
{
    public class AutoFormsEmailAttribute : AutoFormsValidationAttribute
    {
        public AutoFormsEmailAttribute():base(ValidationType.Email) 
        {
        }

        public override bool IsValid(object obj)
        {
            if (obj == null)
                return true;

            var s = obj as string;
            if (s != null && string.IsNullOrEmpty(s))
                return true;

            try
            {
                return Regex.IsMatch(s,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

        }
    }

}
