using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Data;

namespace HrefParser
{
    internal class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Status item)
            {
                return item switch
                {
                    Status.Pending => System.Windows.Media.Brushes.Gray,
                    Status.Failed => System.Windows.Media.Brushes.Red,
                    Status.InProgress => System.Windows.Media.Brushes.Yellow,
                    Status.Completed => System.Windows.Media.Brushes.Green,
                    _ => Color.White,
                };
            }
            throw new InvalidOperationException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
