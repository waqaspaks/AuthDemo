using AuthDemo.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace AuthDemo.MockApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public class GameMatchController : ControllerBase
    {
        private static readonly string[] Teams = new[]
        {
            "Warriors", "Tigers", "Eagles", "Sharks", "Panthers", "Dragons"
        };

        private static readonly string[] Venues = new[]
        {
            "Stadium A", "Stadium B", "Arena C", "Field D"
        };

        private static readonly string[] MatchStatus = new[]
       {
            "Scheduled", "postponed", "cancelled"
        };

        private readonly ILogger<GameMatchController> _logger;

        public GameMatchController(ILogger<GameMatchController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get the list of upcoming game matches.
        /// </summary>
        [HttpGet(Name = "GetGameMatch")]
        [Authorize(Policy = "AdminScope")]
        public IEnumerable<GameMatch> Get()
        {
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            }

            var random = new Random();
            return Enumerable.Range(1, 10).Select(index => new GameMatch
            {
                MatchDate = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TeamA = Teams[random.Next(Teams.Length)],
                TeamB = Teams[random.Next(Teams.Length)],
                Venue = Venues[random.Next(Venues.Length)],
                Status = MatchStatus[random.Next(MatchStatus.Length)],
            })
            .Where(match => match.TeamA != match.TeamB) // avoid same team matchups
            .ToArray();
        }
    }
}
