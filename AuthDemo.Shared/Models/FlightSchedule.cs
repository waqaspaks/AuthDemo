namespace AuthDemo.Shared.Models
{
    /// <summary>
    /// Represents the schedule details of a flight, including its timing, route, and operational status.
    /// </summary>
    public class FlightSchedule
    {
        public TimeOnly DepartureTime { get; set; }
        public string Airline { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Status { get; set; }
        public string Direction { get; set; } // "Departure" or "Arrival"
        public string FlightNumber { get; set; }
        public TimeOnly ArrivalTime { get; set; }
    }
}
