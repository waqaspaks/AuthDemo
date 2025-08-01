namespace AuthDemo.Shared.Models
{
    /// <summary>
    /// Represents the schedule details of a bus, including its timing, route, and operational status.
    /// </summary>
    public class BusSchedule
    {
        public TimeOnly DepartureTime { get; set; }
        public string Company { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Status { get; set; }
        public string Direction { get; set; } // "Departure" or "Arrival"
        public string BusNumber { get; set; }
        public TimeOnly ArrivalTime { get; set; }
    }
}
