
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Datas.Entity;
using Datas.Service.Scores;

namespace Slide_a_LamaWeb.APIController
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoreController : ControllerBase
    {
        private readonly IScoreService _scoreService;
        private readonly ILogger<ScoreController> _logger;

        public ScoreController(IScoreService scoreService, ILogger<ScoreController> logger)
        {
            _scoreService = scoreService ?? throw new ArgumentNullException(nameof(scoreService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: /api/Score
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Score>>> GetScores()
        {
            try
            {
                var scores = await _scoreService.GetTopScoresAsync();
                return Ok(scores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scores via API");
                return StatusCode(500, new { error = "Failed to retrieve scores" });
            }
        }

        // GET: /api/Score/top/{count}
        [HttpGet("top/{count:int}")]
        public async Task<ActionResult<IEnumerable<Score>>> GetTopScores(int count)
        {
            try
            {
                if (count <= 0 || count > 100)
                {
                    return BadRequest(new { error = "Count must be between 1 and 100" });
                }

                var scores = await _scoreService.GetTopScoresAsync(count);
                return Ok(scores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top {Count} scores via API", count);
                return StatusCode(500, new { error = "Failed to retrieve scores" });
            }
        }

        // POST: /api/Score
        [HttpPost]
        public async Task<ActionResult> PostScore([FromBody] Score score)
        {
            try
            {
                if (score == null)
                {
                    return BadRequest(new { error = "Score is required" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _scoreService.AddScoreAsync(score);
                return CreatedAtAction(nameof(GetScores), new { id = score.id }, score);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid score data: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding score via API");
                return StatusCode(500, new { error = "Failed to add score" });
            }
        }

        // DELETE: /api/Score/reset
        [HttpDelete("reset")]
        public async Task<ActionResult> ResetScores()
        {
            try
            {
                await _scoreService.ResetScoreAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting scores via API");
                return StatusCode(500, new { error = "Failed to reset scores" });
            }
        }
    }
}

