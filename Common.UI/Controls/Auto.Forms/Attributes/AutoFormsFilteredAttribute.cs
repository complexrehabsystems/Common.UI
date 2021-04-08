using System;

namespace Common.UI.Controls.Auto.Forms.Attributes
{
    public class AutoFormsFilteredAttribute : Attribute
    {
        public int Filter { get; private set; }

        public AutoFormsFilteredAttribute(int filter = 0)
        {
            Filter = filter;
        }
    }
}
