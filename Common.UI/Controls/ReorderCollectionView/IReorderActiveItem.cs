using System;
using System.Collections.Generic;
using System.Text;

namespace CrsCommon.Controls.ReorderCollectionView
{
    public interface IReorderActiveItem
    {
        bool IsActive { get; set; }
    }
}