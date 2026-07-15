using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TeejoshSystem.Application.Common.Formatting;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Converters;

public sealed class SolesValueConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is decimal amount ? SolesFormatter.Format(amount) : SolesFormatter.Format(0m);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
