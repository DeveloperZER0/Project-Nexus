using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Nexus.Helpers;

/// <summary>
/// Konwerter null/puste → Visibility.
/// Jeśli wartość jest null lub pusty string, zwraca Collapsed.
/// Używany np. do ukrywania preview powiadomień gdy brak tekstu.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string s)
            return string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible;

        return value == null ? Visibility.Collapsed : Visibility.Visible;
    }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

/// <summary>
/// Konwerter bool -> tekst przycisku obserwowania.
/// </summary>
public class FollowButtonTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b && b)
        {
            return "Obserwujesz";
        }

        return "Obserwuj";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Konwerter bool -> Brush dla wyróżnienia zakładek/akcji.
/// ConverterParameter: "Bookmark".
/// </summary>
public class BoolToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var brushKey = (value is bool b && b)
            ? "NexusVioletBrush"
            : "NexusMutedForegroundBrush";

        if (Application.Current.Resources.TryGetValue(brushKey, out var brush))
        {
            return brush;
        }

        return new SolidColorBrush(Microsoft.UI.Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Konwerter bool → HorizontalAlignment.
/// true = Right (wiadomości wysłane przez użytkownika), false = Left.
/// Używany w czacie na MessagesPage.
/// </summary>
public class BoolToAlignmentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b && b)
            return HorizontalAlignment.Right;
        return HorizontalAlignment.Left;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Konwerter bool → Visibility.
/// true = Visible, false = Collapsed.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b && b)
            return Visibility.Visible;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b && b)
            return Visibility.Collapsed;
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Konwerter string (URL lub ścieżka względna) → ImageSource.
/// Obsługuje adresy http(s) oraz lokalne pliki z katalogu uploads (AddPostWithImage).
/// Zwraca null gdy brak adresu — Image po prostu nic nie wyświetli.
/// </summary>
public class StringToImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s))
            return null;

        try
        {
            if (s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                s.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                s.StartsWith("ms-appx", StringComparison.OrdinalIgnoreCase))
            {
                return new BitmapImage(new Uri(s)) { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
            }

            // Ścieżka względna (np. "uploads/xyz.jpg") — zbuduj pełną ścieżkę pliku.
            var fullPath = Path.Combine(AppContext.BaseDirectory, s.Replace('/', Path.DirectorySeparatorChar));
            return new BitmapImage(new Uri(fullPath));
        }
        catch
        {
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
