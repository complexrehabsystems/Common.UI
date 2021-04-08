using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Common.UI.Common
{
    public static class Extensions
    {
        public static ObservableCollection<T> ToObservable<T>(this IEnumerable<T> items)
        {
            var obs = new ObservableCollection<T>();

            foreach (var item in items)
                obs.Add(item);

            return obs;
        }

        public static void ClearAll<T>(this ObservableCollection<T> collection)
        {
            // apparently the safe way to clear all observable collection
            // https://forums.xamarin.com/discussion/19114/invalid-number-of-rows-in-section
            while (collection.Count > 0)
                collection.RemoveAt(0);
        }

        public static void AddAll<T>(this Collection<T> collection, IEnumerable<T> additions)
        {
            foreach(var a in additions)
            {
                collection.Add(a);
            }
        }

        public static void Add(this Grid g, View v)
        {
            g.Children.Add(v);
        }

        public static void Add(this Grid g, View v, int column, int row)
        {
            Grid.SetColumn(v, column);
            Grid.SetRow(v, row);

            g.Children.Add(v);
        }

        public static void Add(this Grid g, View v, int column, int columnSpan, int row, int rowSpan)
        {
            columnSpan = Math.Max(columnSpan, 1);
            rowSpan = Math.Max(rowSpan, 1);

            Grid.SetColumn(v, column);
            Grid.SetColumnSpan(v, columnSpan);
            Grid.SetRow(v, row);
            Grid.SetRowSpan(v, rowSpan);

            g.Children.Add(v);
        }

        public static bool IsColorEqual(this Color clr, Color check)
        {
            if (clr.R == check.R && clr.G == check.G && clr.B == check.B && clr.A == check.A)
                return true;

            return false;
        }

        public static string FirstLetterCapitol(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return s[0].ToString().ToUpper() + s.Substring(1);
        }

        public static void DebugGrid(this Grid grid, Color clr)
        {
            if (grid.ColumnDefinitions.Count <= 0 && grid.RowDefinitions.Count <= 0)
            {

            }
            else if (grid.ColumnDefinitions.Count <= 0)
            {
                AddHorizontalRows(grid, clr);
            }
            else if (grid.RowDefinitions.Count <= 0)
            {
                AddVerticalColumns(grid, clr);
            }
            else
            {
                AddHorizontalRows(grid, clr);
                AddVerticalColumns(grid, clr);
            }
        }

        private static void AddHorizontalRows(Grid grid, Color clr)
        {
            BoxView r;
            for (int row = 0; row < grid.RowDefinitions.Count; row++)
            {
                r = new BoxView();
                r.VerticalOptions = LayoutOptions.Start;
                r.BackgroundColor = clr;
                r.HeightRequest = 1;
                grid.Add(r, 0, grid.ColumnDefinitions.Count, row, 1);
            }

            r = new BoxView();
            r.VerticalOptions = LayoutOptions.End;
            r.BackgroundColor = clr;
            r.HeightRequest = 1;
            grid.Add(r, 0, grid.ColumnDefinitions.Count, grid.RowDefinitions.Count - 1, 1);
        }

        private static void AddVerticalColumns(Grid grid, Color clr)
        {
            BoxView r;
            for (int col = 0; col < grid.ColumnDefinitions.Count; col++)
            {
                r = new BoxView();
                r.HorizontalOptions = LayoutOptions.Start;
                r.BackgroundColor = clr;
                r.WidthRequest = 1;
                grid.Add(r, col, 1, 0, grid.RowDefinitions.Count);
            }

            r = new BoxView();
            r.HorizontalOptions = LayoutOptions.End;
            r.BackgroundColor = clr;
            r.WidthRequest = 1;
            grid.Add(r, grid.ColumnDefinitions.Count-1, 1, 0, grid.RowDefinitions.Count);
        }

        public static async Task<bool> TranslateToDelay(this VisualElement view, double x, double y, uint delay, uint length = 250, Easing easing = null)
        {
            await Task.Delay((int)delay);
            return await view.TranslateTo(x, y, length, easing);
        }
    }
}
