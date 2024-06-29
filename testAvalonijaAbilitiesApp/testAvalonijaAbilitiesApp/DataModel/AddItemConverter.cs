using Avalonia.Data.Converters;
using System.Globalization;
using System;
using ToDoList.DataModel;
using System.Collections.Generic;

namespace MyPersonalConverterNamespace
{
    // Add your namespace here

    public class TaskParametersConverter : IMultiValueConverter
    {

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {


            if (values.Count == 6)
            {

                return Tuple.Create(
                    values[0] as string,
                    values[1] as string,
                    values[2] as TaskType?,
                    values[3] as DateTime?,
                    values[4] as bool?,
                    ParseInt(values[5] as string)
                );
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        static public int? ParseInt(string input)
        {
            if (int.TryParse(input, out int result))
            {
                return result;
            }
            return null;
        }
    }
}