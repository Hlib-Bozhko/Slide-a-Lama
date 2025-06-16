
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Datas.Entity;
using Datas.Service.Rates;

namespace Slide_a_Lama_Web.APIcontrollers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController : ControllerBase
    {
        private readonly IRatingService _ratingService;
        private readonly ILogger<RatingController> _logger;

        public RatingController(IRatingService ratingService, ILogger<RatingController> logger)
        {
            _ratingService = ratingService ?? throw new ArgumentNullException(nameof(ratingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: /api/Rating
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rating>>> GetRatings()
        {
            try
            {
                var ratings = await _ratingService.GetRatesAsync();
                return Ok(ratings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ratings via API");
                return StatusCode(500, new { error = "Failed to retrieve ratings" });
            }
        }

        // POST: /api/Rating
        [HttpPost]
        public async Task<ActionResult> PostRating([FromBody] Rating rating)
        {
            try
            {
                if (rating == null)
                {
                    return BadRequest(new { error = "Rating is required" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _ratingService.AddRateAsync(rating);
                return CreatedAtAction(nameof(GetRatings), new { id = rating.id }, rating);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid rating data: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding rating via API");
                return StatusCode(500, new { error = "Failed to add rating" });
            }
        }

        // DELETE: /api/Rating/reset
        [HttpDelete("reset")]
        public async Task<ActionResult> ResetRatings()
        {
            try
            {
                await _ratingService.ResetRatesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting ratings via API");
                return StatusCode(500, new { error = "Failed to reset ratings" });
            }
        }
    }
}

