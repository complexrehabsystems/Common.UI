﻿using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace CrsCommon.Controls.Auto.Forms.Controls
{
    public class ControlCustom : ControlBase
    {
        public ControlCustom() : base (null)
        { }

        public void InitializeCustom(ControlConfig config)
        {
            _config = config;
        }

        protected override View CreateControl(string bindingName, Type fieldType)
        {
            return null;
        }
    }
}
