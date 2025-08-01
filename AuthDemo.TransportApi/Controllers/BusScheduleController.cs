using AuthDemo.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace AuthDemo.TransportApi.Controllers
{
    /// <summary>
    /// API controller for managing bus schedules.
    /// 
    /// This controller provides endpoints to:
    /// 1. Retrieve a list of upcoming bus schedules.
    /// 2. Retrieve details of a single bus schedule by bus number.
    /// 
    /// Authentication & Authorization:
    /// - Uses OpenIddict for token validation.
    /// - Requires valid authentication via the <see cref="OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme"/>.
    /// - Scope-based authorization is applied to restrict access to specific endpoints:
    ///     - <c>UserScope</c> policy is required for retrieving the bus schedule list.
    ///     - <c>ManagerScope</c> policy is required for retrieving a specific bus by number.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public class BusScheduleController : ControllerBase
    {
        #region Static Data

        /// <summary>
        /// Predefined list of bus companies.
        /// </summary>
        private static readonly string[] BusCompanies = new[]
        {
            "CityBus", "MetroLine", "QuickRide", "RoadRunner", "SkyBus", "HighwayExpress"
        };

        /// <summary>
        /// Predefined list of bus stations.
        /// </summary>
        private static readonly string[] BusStations = new[]
        {
            "Central Station", "North Terminal", "South Depot", "East Hub", "West Point"
        };

        /// <summary>
        /// Possible statuses for a bus schedule.
        /// </summary>
        private static readonly string[] BusStatus = new[]
        {
            "On Time", "Delayed", "Cancelled"
        };

        #endregion

        private readonly ILogger<BusScheduleController> _logger;

        /// <summary>
        /// Constructor for <see cref="BusScheduleController"/>.
        /// </summary>
        /// <param name="logger">The logger instance for logging system events.</param>
        public BusScheduleController(ILogger<BusScheduleController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a list of randomly generated upcoming bus schedules.
        /// </summary>
        /// <remarks>
        /// - Generates 10 random bus schedule records.
        /// - Filters out schedules where the origin and destination are the same.
        /// - Requires the <c>UserScope</c> policy, which validates if the user has the "user.transport.api" scope.
        /// </remarks>
        /// <returns>A collection of <see cref="BusSchedule"/> objects.</returns>
        [HttpGet(Name = "GetBusSchedule")]
        [Authorize(Policy = "UserScope")] // Scope-based authorization (user.transport.api)
        public IEnumerable<BusSchedule> Get()
        {
            var random = new Random();

            // Generate a list of bus schedules while ensuring origin ≠ destination.
            return Enumerable.Range(1, 10)
                             .Select(index => GenerateRandomBus(index, random))
                             .Where(bus => bus.Origin != bus.Destination)
                             .ToArray();
        }

        /// <summary>
        /// Retrieves a specific bus schedule by bus number.
        /// </summary>
        /// <remarks>
        /// - Generates a random bus schedule.
        /// - Replaces the generated bus number with the provided value.
        /// - Requires the <c>ManagerScope</c> policy, which validates if the user has the "manager.transport.api" scope.
        /// </remarks>
        /// <param name="busNumber">The bus number to retrieve.</param>
        /// <returns>
        /// An <see cref="ActionResult{BusSchedule}"/> containing the bus schedule.
        /// </returns>
        [HttpGet("{busNumber}", Name = "GetBusByNumber")]
        [Authorize(Policy = "ManagerScope")] // Scope-based authorization (manager.transport.api)
        public ActionResult<BusSchedule> GetBusByNumber(string busNumber)
        {
            var random = new Random();

            // Generate a random bus schedule
            var bus = GenerateRandomBus(random.Next(1, 50), random);

            // Override the generated bus number with the requested one
            bus.BusNumber = busNumber;

            return Ok(bus);
        }

        /// <summary>
        /// Generates a random bus schedule record.
        /// </summary>
        /// <param name="index">The index used for generating the schedule's timing.</param>
        /// <param name="random">An instance of <see cref="Random"/> for generating random values.</param>
        /// <returns>A randomly generated <see cref="BusSchedule"/>.</returns>
        private BusSchedule GenerateRandomBus(int index, Random random)
        {
            // Calculate departure and arrival times
            var departure = DateTime.Now.AddHours(index);
            var tripDuration = TimeSpan.FromHours(random.Next(1, 4)); // 1–3 hours trip duration
            var arrival = departure.Add(tripDuration);

            return new BusSchedule
            {
                BusNumber = $"BUS-{index:000}",                               // Unique bus number (formatted)
                DepartureTime = TimeOnly.FromDateTime(departure),            // Departure time
                ArrivalTime = TimeOnly.FromDateTime(arrival),                // Arrival time
                Company = BusCompanies[random.Next(BusCompanies.Length)],    // Random bus company
                Origin = BusStations[random.Next(BusStations.Length)],       // Random origin station
                Destination = BusStations[random.Next(BusStations.Length)],  // Random destination station
                Status = BusStatus[random.Next(BusStatus.Length)],           // Random bus status
                Direction = random.Next(2) == 0 ? "Departure" : "Arrival"    // Random direction
            };
        }
    }
}
