namespace AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
/// Provides extension methods for <see cref="string"/> value processing and manipulation.
/// </summary>
public static class StringExtensions
{
    #region Methods

    /// <summary>
    /// Converts a string identifier (e.g., PascalCase) into its equivalent snake_case representation.
    /// </summary>
    /// <param name="text">The source string to convert.</param>
    /// <returns>The converted snake_case formatted string text, or the original value if null or empty.</returns>
    public static string ToSnakeCase(this string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var builder = new System.Text.StringBuilder(text.Length + Math.Min(2, text.Length / 5));
        var previousCategory = default(System.Globalization.UnicodeCategory?);
        
        for (var currentIndex = 0; currentIndex < text.Length; currentIndex++)
        {
            var currentChar = text[currentIndex];
            if (currentChar == '_')
            {
                builder.Append('_');
                previousCategory = null;
                continue;
            }
            var currentCategory = char.GetUnicodeCategory(currentChar);
            switch (currentCategory)
            {
                case System.Globalization.UnicodeCategory.UppercaseLetter:
                case System.Globalization.UnicodeCategory.TitlecaseLetter:
                    if (previousCategory == System.Globalization.UnicodeCategory.SpaceSeparator ||
                        previousCategory == System.Globalization.UnicodeCategory.LowercaseLetter ||
                        previousCategory != System.Globalization.UnicodeCategory.DecimalDigitNumber &&
                        previousCategory != null &&
                        currentIndex > 0 &&
                        currentIndex + 1 < text.Length &&
                        char.IsLower(text[currentIndex + 1]))
                    {
                        builder.Append('_');
                    }
                    currentChar = char.ToLower(currentChar);
                    break;
                case System.Globalization.UnicodeCategory.LowercaseLetter:
                case System.Globalization.UnicodeCategory.DecimalDigitNumber:
                    if (previousCategory == System.Globalization.UnicodeCategory.SpaceSeparator)
                    {
                        builder.Append('_');
                    }
                    break;
                default:
                    if (previousCategory != null)
                    {
                        previousCategory = System.Globalization.UnicodeCategory.SpaceSeparator;
                    }
                    continue;
            }
            builder.Append(currentChar);
            previousCategory = currentCategory;
        }
        return builder.ToString();
    }

    #endregion
}