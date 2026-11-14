using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using static MyToDo.Common.SnackBar;
using static MyToDo.ViewModels.LoginViewModel;

namespace MyToDo.Converters
{
    public class SnackBarKindToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SnackBarEnum kind)
            {
                return kind switch
                {
                    SnackBarEnum.Success => "#FF0CA0FF", // 蓝
                    SnackBarEnum.Warning => "#FFEF6C00", // 橙
                    SnackBarEnum.Error => "#FFC62828", // 红
                    SnackBarEnum.Info => "Chartreuse", // 绿
                    _ => "#EE1E1E1E"   // 默认深灰
                };
            }
            return "#EE1E1E1E";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}