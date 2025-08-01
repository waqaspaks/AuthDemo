using AuthDemo.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace AuthDemo.SportsApi.Controllers
{
    /// <summary>
    /// Controller for managing and retrieving information about sports game matches.
    /// This API is protected using OpenIddict token validation and requires appropriate scopes or policies for access.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public class GameMatchController : ControllerBase
    {
        /// <summary>
        /// Predefined list of teams participating in matches.
        /// </summary>
        private static readonly string[] Teams = new[]
        {
            "Warriors", "Tigers", "Eagles", "Sharks", "Panthers", "Dragons"
        };

        /// <summary>
        /// Predefined list of venues where matches can be held.
        /// </summary>
        private static readonly string[] Venues = new[]
        {
            "Stadium A", "Stadium B", "Arena C", "Field D"
        };

        /// <summary>
        /// Predefined list of possible match statuses.
        /// </summary>
        private static readonly string[] MatchStatus = new[]
        {
            "Scheduled", "Postponed", "Cancelled"
        };

        private readonly ILogger<GameMatchController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameMatchController"/> class.
        /// </summary>
        /// <param name="logger">Logger instance used for logging information and debugging.</param>
        public GameMatchController(ILogger<GameMatchController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a list of upcoming game matches.
        /// </summary>
        /// <remarks>
        /// - Requires an access token with the <c>AdminScope</c> policy.
        /// - The API randomly generates 10 upcoming matches for testing purposes.
        /// - Each match includes details such as date, teams, venue, and status.
        /// </remarks>
        /// <returns>
        /// A collection of <see cref="GameMatch"/> objects representing upcoming matches.
        /// </returns>
        /// <response code="200">Returns the list of matches successfully.</response>
        /// <response code="401">Unauthorized if the request does not contain a valid token.</response>
        [HttpGet(Name = "GetGameMatch")]
        [Authorize(Policy = "AdminScope")]
        public IEnumerable<GameMatch> Get()
        {
            // Log all user claims for debugging (useful for verifying token scopes, roles, etc.)
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            }

            var random = new Random();

            // Generate 10 random matches
            return Enumerable.Range(1, 10).Select(index => new GameMatch
            {
                // Set match date to a future date (index days from today)
                MatchDate = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),

                // Randomly select Team A and Team B (ensuring they're not the same later)
                TeamA = Teams[random.Next(Teams.Length)],
                TeamB = Teams[random.Next(Teams.Length)],

                // Randomly assign a venue
                Venue = Venues[random.Next(Venues.Length)],

                // Randomly assign a match status
                Status = MatchStatus[random.Next(MatchStatus.Length)],
            })
            // Ensure that the same team does not play against itself
            .Where(match => match.TeamA != match.TeamB)
            .ToArray();
        }
    }
}
