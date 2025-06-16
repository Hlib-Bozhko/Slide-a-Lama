using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Datas.Entity;
using Datas.Service.Scores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Slide_a_LamaWeb.Controllers
{
    public class ScoresController : Controller
    {
        private readonly IScoreService _scoreService;
        private readonly ILogger<ScoresController> _logger;

        public ScoresController(IScoreService scoreService, ILogger<ScoresController> logger)
        {
            _scoreService = scoreService ?? throw new ArgumentNullException(nameof(scoreService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var scores = await _scoreService.GetTopScoresAsync();
                ViewBag.ScoreService = _scoreService; 
                return View("Index", scores); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading scores page");
                ViewBag.ErrorMessage = "Unable to load scores. Please try again later.";
                ViewBag.ScoreService = _scoreService;
                return View("Index", new List<Score>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddScore(string playerName, int points)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(playerName))
                {
                    ModelState.AddModelError("", "Player name is required");
                    var scores = await _scoreService.GetTopScoresAsync();
                    ViewBag.ScoreService = _scoreService;
                    return View("Index", scores);
                }

                var score = new Score { Player = playerName, Points = points };
                await _scoreService.AddScoreAsync(score);
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding score for player {Player}", playerName);
                ModelState.AddModelError("", "Failed to add score. Please try again.");
                var scores = await _scoreService.GetTopScoresAsync();
                ViewBag.ScoreService = _scoreService;
                return View("Index", scores);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetScores()
        {
            try
            {
                await _scoreService.ResetScoreAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting scores");
                ModelState.AddModelError("", "Failed to reset scores. Please try again.");
                var scores = await _scoreService.GetTopScoresAsync();
                ViewBag.ScoreService = _scoreService;
                return View("Index", scores);
            }
        }
    }
}