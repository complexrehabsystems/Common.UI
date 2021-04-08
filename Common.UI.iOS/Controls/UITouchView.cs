using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace CrsCommon.UI.iOS.Controls
{
    public class UITouchView: UIView
    {
        public event Action OnTouchBegin;
        public event Action OnTouchEnd;
        //public event Action OnTouchMoved;
        public event Action OnTouchCancelled;

        public UITouchView()
        {
            //ExclusiveTouch = true;
            //MultipleTouchEnabled = false;
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            OnTouchBegin?.Invoke();
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);

            OnTouchCancelled?.Invoke();
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            OnTouchEnd?.Invoke();
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

        }
    }
}