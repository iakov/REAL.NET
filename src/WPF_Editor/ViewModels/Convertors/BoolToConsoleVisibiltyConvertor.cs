﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace WPF_Editor.ViewModels.Convertors
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class BoolToConsoleVisibiltyConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visibility = (bool) value;
            return visibility ? "Visible" : "Collapsed";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string visibility = (string) value;
            return visibility == "Visible";
        }
    }
}