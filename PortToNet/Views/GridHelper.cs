using System.Windows;
using System.Windows.Controls;

namespace PortToNet.Views
{
    public class GridHelper
    {
        #region Rows 

        public static string GetRows(DependencyObject obj)
        {
            return (string)obj.GetValue(RowsProperty);
        }

        public static void SetRows(DependencyObject obj, string value)
        {
            obj.SetValue(RowsProperty, value);
        }

        /// <summary>
        /// 自动为 Grid 控件添加 RowDefinitions
        /// </summary>
        /// <remarks>
        /// 支持的写法包括：<br/>
        /// - Star: 1*, *<br/>
        /// - Auto: auto, Auto<br/>
        /// - Pixel: 100, 10.5, 0
        /// </remarks>
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.RegisterAttached("Rows", typeof(string), typeof(GridHelper),
                new FrameworkPropertyMetadata("",
                    FrameworkPropertyMetadataOptions.AffectsRender
                        | FrameworkPropertyMetadataOptions.AffectsMeasure
                        | FrameworkPropertyMetadataOptions.NotDataBindable, OnRowsChanged
                )
            );

        private static void OnRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Grid grid || e.NewValue is not string rows)
                throw new ArgumentException($"Invalid Rows property value: {e.NewValue}");

            grid.RowDefinitions.Clear();

            foreach (var row in rows.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var gridLength = ParseGridLength(row);
                grid.RowDefinitions.Add(new RowDefinition { Height = gridLength });
            }
        }
        #endregion

        #region   Columns

        public static string GetColumns(DependencyObject obj)
        {
            return (string)obj.GetValue(ColumnsProperty);
        }

        public static void SetColumns(DependencyObject obj, string value)
        {
            obj.SetValue(ColumnsProperty, value);
        }

        /// <summary>
        /// 自动为 Grid 控件添加 ColumnDefinitions。详见 <seealso cref="RowsProperty"/>
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.RegisterAttached("Columns", typeof(string), typeof(GridHelper),
                new FrameworkPropertyMetadata("",
                    FrameworkPropertyMetadataOptions.AffectsRender
                        | FrameworkPropertyMetadataOptions.AffectsMeasure
                        | FrameworkPropertyMetadataOptions.NotDataBindable, OnColumnsChanged
                )
            );

        private static void OnColumnsChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            if (d is not Grid grid || e.NewValue is not string rows)
                throw new ArgumentException($"Invalid Rows property value: {e.NewValue}");

            grid.ColumnDefinitions.Clear();

            foreach (var row in rows.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var gridLength = ParseGridLength(row);
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength });
            }
        }

        #endregion

        private static GridLength ParseGridLength(string length)
        {
            // *, 1*, 2.5*
            if (length.EndsWith("*"))
            {
                double star = 1;
                if (length.Length > 1)
                    star = double.Parse(length.Substring(0, length.Length - 1));
                return new GridLength(star, GridUnitType.Star);
            }
            // a, auto, A, Auto
            else if (length.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                return GridLength.Auto;
            }
            // 100
            else if (double.TryParse(length, out var height))
            {
                return new GridLength(height);
            }

            throw new ArgumentException($"Invalid height value: {length}");
        }

    }
}