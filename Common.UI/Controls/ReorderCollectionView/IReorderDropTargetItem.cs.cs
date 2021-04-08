using System;
using System.Collections.Generic;
using System.Text;

namespace CrsCommon.Controls.ReorderCollectionView
{
    public interface IReorderDropTargetItem
    {
        bool IsActive { get; set; }
    }
}
