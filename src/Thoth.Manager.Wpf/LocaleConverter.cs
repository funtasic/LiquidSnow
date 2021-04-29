﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Thismaker.Thoth.Manager.Wpf
{
    class LocaleConverter : IValueConverter
    {
        public static MainViewModel Model { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Model == null) return null;

            var id = (string)value;
            var name = Model.Locales.FirstOrDefault(x => x.ID == id)?.Name;

            if (string.IsNullOrEmpty(name)) name = "Unknown Locale";

            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
