using System.Globalization;

namespace TeejoshSystem.Application.Common.Formatting;

public static class SolesFormatter
{
    public static string Format(decimal value) => $"S/ {value.ToString("N2", CultureInfo.InvariantCulture)}";
}
