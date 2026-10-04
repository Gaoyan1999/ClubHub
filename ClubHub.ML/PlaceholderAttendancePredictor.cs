using ClubHub.Core.Interfaces;
using ClubHub.Core.Models;

namespace ClubHub.ML;

/// <summary>
/// Temporary predictor so the app runs end to end.
/// TODO: replace with an ML.NET regression model trained on past events (see PROJECT_PLAN.md section 5, F15).
/// </summary>
public class PlaceholderAttendancePredictor : IAttendancePredictor
{
    private const double AssumedShowUpRate = 0.75;

    public int PredictAttendance(Event evt, int rsvpCount) => (int)Math.Round(rsvpCount * AssumedShowUpRate);
}
