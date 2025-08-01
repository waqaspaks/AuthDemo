namespace AuthDemo.Shared.Models
{
    /// <summary>
    /// Represents the details of a game match, including teams, date, venue, and status.
    /// </summary>
    public class GameMatch
    {
        public DateOnly MatchDate { get; set; }
        public string TeamA { get; set; }
        public string TeamB { get; set; }
        public string Venue { get; set; }
        public string Status { get; set; } // e.g. Scheduled, Completed, Live
    }
}
