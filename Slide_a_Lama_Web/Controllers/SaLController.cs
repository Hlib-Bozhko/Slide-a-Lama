
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Slide_a_Lama;
using Slide_a_Lama.Core;
using Datas.Entity;
using Datas.Service.Scores;

namespace Slide_a_Lama_Web.Controllers
{
    public class SaLController : Controller
    {
        private const string FieldSessionKey = "field";
        
        private readonly IScoreService _scoreService;
        private readonly ILogger<SaLController> _logger;

        public SaLController(IScoreService scoreService, ILogger<SaLController> logger)
        {
            _scoreService = scoreService ?? throw new ArgumentNullException(nameof(scoreService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public IActionResult Index(int playerCount, int winScore)
        {
            try
            {
                _logger.LogInformation("Starting new game with {PlayerCount} players and win score {WinScore}", 
                    playerCount, winScore);
                
                FieldAdapter field = FieldAdapterFactory.CreateField(7, 8, playerCount, winScore);
                field.AddCube();
                
                HttpContext.Session.SetObject(FieldSessionKey, field);
                return View("Index", field);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting new game");
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Move(int row, int column)
        {
            try
            {
                var field = (FieldAdapter)HttpContext.Session.GetObject(FieldSessionKey);
                
                if (field == null)
                {
                    _logger.LogWarning("Game session not found, redirecting to home");
                    return RedirectToAction("Index", "Home");
                }

                if (!field.HasActiveCube())
                {
                    field.AddCube();
                }

                if (field.CanMoveTo(row, column))
                {
                    field.MoveCurCube(row, column);
                    _logger.LogDebug("Moved cube to position ({Row}, {Column})", row, column);
                }

                HttpContext.Session.SetObject(FieldSessionKey, field);
                return View("Index", field);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving cube to position ({Row}, {Column})", row, column);
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Put()
        {
            try
            {
                var field = (FieldAdapter)HttpContext.Session.GetObject(FieldSessionKey);
                
                if (field == null)
                {
                    _logger.LogWarning("Game session not found, redirecting to home");
                    return RedirectToAction("Index", "Home");
                }

                field.PutCube();
                
                field.ForceDropAllCubes();
                
                int iterations = 0;
                while (field.UpdateField() && iterations < 20)
                {
                    iterations++;
                }

                _logger.LogDebug("Field updated with {Iterations} iterations", iterations);

                if (field.GameState == GameState.PLAYING)
                {
                    field.AddCube();
                }

                HttpContext.Session.SetObject(FieldSessionKey, field);
                return View("Index", field);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error putting cube");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveScore(string Name)
        {
            try
            {
                var field = (FieldAdapter)HttpContext.Session.GetObject(FieldSessionKey);
                
                if (field != null && !string.IsNullOrWhiteSpace(Name))
                {
                    var score = new Score()
                    {
                        Player = Name.Trim(), 
                        Points = field.GetScore()
                    };

                    await _scoreService.AddScoreAsync(score);
                    _logger.LogInformation("Score saved for player {Player} with {Points} points", 
                        Name, score.Points);
                    
                    field.ToMenu = true;
                }
                else
                {
                    _logger.LogWarning("Invalid score save attempt - field: {FieldExists}, name: '{Name}'", 
                        field != null, Name);
                }

                return View("Index", field);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving score for player {Player}", Name);
                
                var field = (FieldAdapter)HttpContext.Session.GetObject(FieldSessionKey);
                ViewBag.ErrorMessage = "Failed to save score. Please try again.";
                return View("Index", field);
            }
        }

        [HttpPost]
        public JsonResult ForceUpdate()
        {
            try
            {
                var field = (FieldAdapter)HttpContext.Session.GetObject(FieldSessionKey);
                
                if (field == null)
                {
                    _logger.LogWarning("Force update called but game not found");
                    return Json(new { success = false, message = "Game not found" });
                }

                field.ForceDropAllCubes();
                
                bool isValid = field.ValidateFieldIntegrity();
                
                int updates = 0;
                while (field.UpdateField() && updates < 10)
                {
                    updates++;
                }

                HttpContext.Session.SetObject(FieldSessionKey, field);
                
                _logger.LogDebug("Force update completed - valid: {IsValid}, updates: {Updates}", 
                    isValid, updates);
                
                return Json(new 
                { 
                    success = true,
                    isValid = isValid,
                    updates = updates,
                    gameState = field.GameState.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during force update");
                return Json(new { success = false, message = "Update failed" });
            }
        }

        [HttpGet]
        public JsonResult GetGameState()
        {
            try
            {
                var field = (FieldAdapter)HttpContext.Session.GetObject(FieldSessionKey);
        
                if (field == null)
                {
                    _logger.LogWarning("Get game state called but game not found");
                    return Json(new { success = false, message = "Game not found" });
                }

                return Json(new 
                { 
                    success = true,
                    hasActiveCube = field.HasActiveCube(),
                    canPutCube = field.CanPutCube(),
                    currentCubeValue = field.GetCurrentCubeValue(),
                    currentCubePosition = new { row = field.CurrentCube[0], column = field.CurrentCube[1] },
                    validMoves = field.GetValidMoves(),
                    gameState = field.GameState.ToString(),
                    currentPlayer = field.CurrentPlayer.Team,
                    score = field.GetScore()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting game state");
                return Json(new { success = false, message = "Failed to get game state" });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetTopScores(int count = 10)
        {
            try
            {
                var scores = await _scoreService.GetTopScoresAsync(count);
                return Json(new { success = true, scores = scores });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top scores");
                return Json(new { success = false, message = "Failed to get scores" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ResetScores()
        {
            try
            {
                await _scoreService.ResetScoreAsync();
                _logger.LogWarning("Scores have been reset");
                return Json(new { success = true, message = "Scores reset successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting scores");
                return Json(new { success = false, message = "Failed to reset scores" });
            }
        }
    }
}

