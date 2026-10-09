using ClubHub.Core.Interfaces;
using ClubHub.Core.Models;

namespace ClubHub.ML;

/// <summary>
/// Simple rule: about 3 in 4 people who register actually turn up.
/// The planned ML.NET model was cut for time; it can replace this class through <see cref="IAttendancePredictor"/>.
/// </summary>
public class PlaceholderAttendancePredictor : IAttendancePredictor
{
    private const double AssumedShowUpRate = 0.75;

    public int PredictAttendance(Event evt, int rsvpCount) => (int)Math.Round(rsvpCount * AssumedShowUpRate);
}
