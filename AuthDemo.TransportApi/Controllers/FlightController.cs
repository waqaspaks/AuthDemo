using AuthDemo.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace AuthDemo.TransportApi.Controllers
{
    /// <summary>
    /// API controller for managing flight schedules.
    /// 
    /// This controller provides endpoints to:
    /// 1. Retrieve a list of upcoming flight schedules.
    /// 2. Retrieve a single flight schedule by flight number.
    /// 
    /// Authentication & Authorization:
    /// - Uses OpenIddict for token validation.
    /// - Requires authentication using <see cref="OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme"/>.
    /// - Scope-based authorization is enforced:
    ///     - <c>UserScope</c> policy (user.transport.api) is required for fetching flight schedules.
    ///     - <c>ManagerScope</c> policy (manager.transport.api) is required for fetching a flight by number.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public class FlightScheduleController : ControllerBase
    {
        #region Static Data

        /// <summary>
        /// Predefined list of airlines.
        /// </summary>
        private static readonly string[] Airlines = new[]
        {
            "Air Express", "SkyJet", "FlyFast", "AeroWings", "CloudAir", "JetStream"
        };

        /// <summary>
        /// Predefined list of airport codes.
        /// </summary>
        private static readonly string[] Airports = new[]
        {
            "JFK", "LAX", "ORD", "ATL", "DFW", "DXB"
        };

        /// <summary>
        /// Possible statuses for a flight.
        /// </summary>
        private static readonly string[] FlightStatus = new[]
        {
            "On Time", "Delayed", "Cancelled"
        };

        #endregion

        private readonly ILogger<FlightScheduleController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlightScheduleController"/> class.
        /// </summary>
        /// <param name="logger">Logger instance for logging controller activities.</param>
        public FlightScheduleController(ILogger<FlightScheduleController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a list of upcoming flight schedules.
        /// </summary>
        /// <remarks>
        /// - Generates 10 random flight schedule records.
        /// - Filters out flights where the origin and destination airports are the same.
        /// - Requires <c>UserScope</c> policy for access (scope: user.transport.api).
        /// </remarks>
        /// <returns>A collection of <see cref="FlightSchedule"/> objects.</returns>
        [HttpGet(Name = "GetFlightSchedule")]
        [Authorize(Policy = "UserScope")] // Requires user.transport.api scope
        public IEnumerable<FlightSchedule> Get()
        {
            var random = new Random();

            // Generate 10 random flights and remove those with the same origin and destination.
            return Enumerable.Range(1, 10)
                             .Select(index => GenerateRandomFlight(index, random))
                             .Where(flight => flight.Origin != flight.Destination)
                             .ToArray();
        }

        /// <summary>
        /// Retrieves details of a specific flight by its flight number.
        /// </summary>
        /// <remarks>
        /// - Generates a random flight schedule.
        /// - Replaces the randomly generated flight number with the requested one.
        /// - Requires <c>ManagerScope</c> policy for access (scope: manager.transport.api).
        /// </remarks>
        /// <param name="flightNumber">The flight number to retrieve.</param>
        /// <returns>
        /// An <see cref="ActionResult{FlightSchedule}"/> containing the requested flight schedule.
        /// </returns>
        [HttpGet("{flightNumber}", Name = "GetFlightByNumber")]
        [Authorize(Policy = "ManagerScope")] // Requires manager.transport.api scope
        public ActionResult<FlightSchedule> GetFlightByNumber(string flightNumber)
        {
            var random = new Random();

            // Generate a random flight schedule
            var flight = GenerateRandomFlight(random.Next(1, 50), random);

            // Override the randomly generated flight number with the requested one.
            flight.FlightNumber = flightNumber;

            return Ok(flight);
        }

        /// <summary>
        /// Generates a random flight schedule.
        /// </summary>
        /// <param name="index">The index used to generate time offsets for departure and arrival.</param>
        /// <param name="random">An instance of <see cref="Random"/> for generating random values.</param>
        /// <returns>A randomly generated <see cref="FlightSchedule"/>.</returns>
        private FlightSchedule GenerateRandomFlight(int index, Random random)
        {
            // Calculate departure and arrival times based on index
            var departure = DateTime.Now.AddHours(index * 2);
            var flightDuration = TimeSpan.FromHours(random.Next(2, 6)); // 2–5 hours flight duration
            var arrival = departure.Add(flightDuration);

            return new FlightSchedule
            {
                FlightNumber = $"FL-{index:000}",                           // Flight number format
                DepartureTime = TimeOnly.FromDateTime(departure),          // Departure time
                ArrivalTime = TimeOnly.FromDateTime(arrival),              // Arrival time
                Airline = Airlines[random.Next(Airlines.Length)],          // Random airline
                Origin = Airports[random.Next(Airports.Length)],           // Random origin airport
                Destination = Airports[random.Next(Airports.Length)],      // Random destination airport
                Status = FlightStatus[random.Next(FlightStatus.Length)],   // Random status
                Direction = random.Next(2) == 0 ? "Departure" : "Arrival"  // Random direction
            };
        }
    }
}
