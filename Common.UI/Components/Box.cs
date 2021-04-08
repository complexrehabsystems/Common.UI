using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Urho;
using Urho.Shapes;
using CrsCommon.Controls;

namespace CrsCommon.Components
{
    public class Box : Urho.Shapes.Box
    {
        public Box()
        {
            
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            this.Color = Color.Blue;
            Node.SetScale(.5f);


        }

        
    }
}
