using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Controls.ReorderCollectionView
{
    public interface IReorderGroupItem
    {
        bool CanMove { get; }
    }
}
