using ClubHub.Core.Models;

namespace ClubHub.Core.Interfaces;

public interface IAttendancePredictor
{
    /// <summary>Predicts how many people will actually turn up to the event.</summary>
    int PredictAttendance(Event evt, int rsvpCount);
}
