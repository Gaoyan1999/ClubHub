using System.Globalization;
using System.Windows.Data;

namespace ClubHub.Wpf.Converters;

/// <summary>
/// Used by the calendar to highlight days that have events.
/// Values: [0] the calendar day (DateTime), [1] the set of event days.
/// </summary>
public class EventDayConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return values.Length == 2
               && values[0] is DateTime day
               && values[1] is ISet<DateTime> eventDays
               && eventDays.Contains(day.Date);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
