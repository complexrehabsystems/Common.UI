using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Common.Controls
{
    public class InputHandler : ContentView
    {
        public struct TouchEventArgs
        {
            public int X { get; set; }
            //
            public int Y { get; set; }

            public int DX { get; set; }

            public int DY { get; set; }
        }

        public struct MultiTouchEventArgs
        {
            public double f1StartX { get; set; }
            public double f1StartY { get; set; }
            public double f2StartX { get; set; }
            public double f2StartY { get; set; }
            public double f1EndX { get; set; }
            public double f1EndY { get; set; }
            public double f2EndX { get; set; }
            public double f2EndY { get; set; }
        }

        public Action<int, double, double> MouseWheelPressed;
        public Action<double, double> RightMouseClickMoved;
        public Action MouseEntered;
        public Action MouseExited;
        public Action<TouchEventArgs> TouchMove;
        public Action<TouchEventArgs> TouchEnd;
        public Action<TouchEventArgs> TouchBegin;
        public Action<TouchEventArgs> TouchCancelled;
        public Action<MultiTouchEventArgs> MultiTouchDelta;
    }
}
