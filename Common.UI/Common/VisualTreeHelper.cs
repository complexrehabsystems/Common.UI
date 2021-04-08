using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xamarin.Forms;

namespace CrsCommon.Common
{
    public static class VisualTreeHelper
    {
        public static Point RelativeTo(this VisualElement view, VisualElement parent)
        {
            var screenCoordinateX = view.X;
            var screenCoordinateY = view.Y;

            var currentParent = view.Parent as VisualElement;
            bool foundParent = false;
            while (currentParent != null)
            {
                if (parent == currentParent)
                {
                    foundParent = true;
                    break;
                }

                screenCoordinateX += currentParent.X;
                screenCoordinateY += currentParent.Y;
                                
                currentParent = currentParent.Parent as VisualElement;
            }

            if (foundParent)
                return new Point(screenCoordinateX, screenCoordinateY);
            else
                return new Point(view.X, view.Y);
        }
        public static Point GetScreenCoordinates(this VisualElement view)
        {
            // A view's default X- and Y-coordinates are LOCAL with respect to the boundaries of its parent,
            // and NOT with respect to the screen. This method calculates the SCREEN coordinates of a view.
            // The coordinates returned refer to the top left corner of the view.
            var screenCoordinateX = view.X;
            var screenCoordinateY = view.Y;

            var parent = view.Parent as VisualElement;
            while (parent != null )
            {
                screenCoordinateX += parent.X;
                screenCoordinateY += parent.Y;
                parent = parent.Parent as VisualElement;
            }
            return new Point(screenCoordinateX, screenCoordinateY);
        }

        public static T GetParent<T>(this Element element) where T : Element
        {
            if (element is T)
            {
                return element as T;
            }
            else
            {
                if (element.Parent != null)
                {
                    return element.Parent.GetParent<T>();
                }

                return default(T);
            }
        }

        public static IEnumerable<T> GetChildren<T>(this Element element) where T : Element
        {
            var properties = element.GetType().GetRuntimeProperties();

            // try to parse the Content property
            var contentProperty = properties.FirstOrDefault(w => w.Name == "Content");
            if (contentProperty != null)
            {
                var content = contentProperty.GetValue(element) as Element;
                if (content != null)
                {
                    if (content is T)
                    {
                        yield return content as T;
                    }
                    foreach (var child in content.GetChildren<T>())
                    {
                        yield return child;
                    }
                }
            }
            else
            {
                // try to parse the Children property
                var childrenProperty = properties.FirstOrDefault(w => w.Name == "Children");
                if (childrenProperty != null)
                {
                    // loop through children
                    IEnumerable children = childrenProperty.GetValue(element) as IEnumerable;
                    foreach (var child in children)
                    {
                        var childVisualElement = child as Element;
                        if (childVisualElement != null)
                        {
                            // return match
                            if (childVisualElement is T)
                            {
                                yield return childVisualElement as T;
                            }

                            // return recursive results of children
                            foreach (var childVisual in childVisualElement.GetChildren<T>())
                            {
                                yield return childVisual;
                            }
                        }
                    }
                }
            }
        }
    }
}
