﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CrsCommon.Controls.Auto.Forms
{
    public enum AutoFormsActionType
    {
        None = 0,
        Edit = 1 << 0,
        View = 1 << 1,
        Delete = 1 << 2,
    }
}
