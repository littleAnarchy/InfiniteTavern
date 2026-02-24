using InfiniteTavern.Application.Models;
using InfiniteTavern.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace InfiniteTavern.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly ILogger<GameController> _logger;

    public GameController(IGameService gameService, ILogger<GameController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new game session
    /// </summary>
    [HttpPost("new-game")]
    [ProducesResponseType(typeof(NewGameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NewGameResponse>> CreateNewGame([FromBody] NewGameRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PlayerName) || 
                string.IsNullOrWhiteSpace(request.CharacterName))
            {
                return BadRequest("Player name and character name are required");
            }

            var response = await _gameService.CreateNewGameAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new game");
            return StatusCode(500, "An error occurred while creating the game");
        }
    }

    /// <summary>
    /// Process a player turn
    /// </summary>
    [HttpPost("turn")]
    [ProducesResponseType(typeof(TurnResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TurnResponse>> ProcessTurn([FromBody] TurnRequest request)
    {
        try
        {
            if (request.GameSessionId == Guid.Empty)
            {
                return BadRequest("Invalid game session ID");
            }

            if (string.IsNullOrWhiteSpace(request.PlayerAction))
            {
                return BadRequest("Player action is required");
            }

            var response = await _gameService.ProcessTurnAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during turn processing");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing turn");
            return StatusCode(500, "An error occurred while processing the turn");
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
