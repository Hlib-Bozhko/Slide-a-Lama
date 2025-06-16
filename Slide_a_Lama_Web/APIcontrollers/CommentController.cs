
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Datas.Entity;
using Datas.Service.Comments;

namespace Slide_a_Lama_Web.APIcontrollers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentController> _logger;

        public CommentController(ICommentService commentService, ILogger<CommentController> logger)
        {
            _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: /api/Comment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments()
        {
            try
            {
                var comments = await _commentService.GetCommentsAsync();
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments via API");
                return StatusCode(500, new { error = "Failed to retrieve comments" });
            }
        }

        // POST: /api/Comment
        [HttpPost]
        public async Task<ActionResult> PostComment([FromBody] Comment comment)
        {
            try
            {
                if (comment == null)
                {
                    return BadRequest(new { error = "Comment is required" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _commentService.AddCommentAsync(comment);
                return CreatedAtAction(nameof(GetComments), new { id = comment.Id }, comment);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid comment data: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment via API");
                return StatusCode(500, new { error = "Failed to add comment" });
            }
        }

        // DELETE: /api/Comment/reset
        [HttpDelete("reset")]
        public async Task<ActionResult> ResetComments()
        {
            try
            {
                await _commentService.ResetCommentsAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting comments via API");
                return StatusCode(500, new { error = "Failed to reset comments" });
            }
        }
    }
}

