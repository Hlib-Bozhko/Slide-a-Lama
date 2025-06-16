using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Datas.Entity;
using Datas.Service.Comments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Slide_a_LamaWeb.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(ICommentService commentService, ILogger<CommentsController> logger)
        {
            _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var comments = await _commentService.GetCommentsAsync();
                ViewBag.CommentService = _commentService; 
                return View("Index", comments); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading comments page");
                ViewBag.ErrorMessage = "Unable to load comments. Please try again later.";
                ViewBag.CommentService = _commentService;
                return View("Index", new List<Comment>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveComment(string Name, string Text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Text))
                {
                    ModelState.AddModelError("", "Both name and comment text are required");
                    var comments = await _commentService.GetCommentsAsync();
                    ViewBag.CommentService = _commentService;
                    return View("Index", comments);
                }

                var comment = new Comment { Name = Name, Text = Text };
                await _commentService.AddCommentAsync(comment);
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment for user {Name}", Name);
                ModelState.AddModelError("", "Failed to add comment. Please try again.");
                var comments = await _commentService.GetCommentsAsync();
                ViewBag.CommentService = _commentService;
                return View("Index", comments);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetComments()
        {
            try
            {
                await _commentService.ResetCommentsAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting comments");
                ModelState.AddModelError("", "Failed to reset comments. Please try again.");
                var comments = await _commentService.GetCommentsAsync();
                ViewBag.CommentService = _commentService;
                return View("Index", comments);
            }
        }
    }
}