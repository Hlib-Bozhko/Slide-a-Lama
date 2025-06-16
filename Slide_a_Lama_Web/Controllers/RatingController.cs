using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Datas.Entity;
using Datas.Service.Rates;

namespace Slide_a_LamaWeb.Controllers
{
    public class RatingController : Controller
    {
        private readonly IRatingService _ratingService;
        private readonly ILogger<RatingController> _logger;

        public RatingController(IRatingService ratingService, ILogger<RatingController> logger)
        {
            _ratingService = ratingService ?? throw new ArgumentNullException(nameof(ratingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var ratings = await _ratingService.GetRatesAsync();
                ViewBag.RatingService = _ratingService; 
                return View("Index", ratings); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ratings page");
                ViewBag.ErrorMessage = "Unable to load ratings. Please try again later.";
                ViewBag.RatingService = _ratingService;
                return View("Index", new List<Rating>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveRate(string Name, int Mark)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    ModelState.AddModelError("", "Name is required");
                    var ratings = await _ratingService.GetRatesAsync();
                    ViewBag.RatingService = _ratingService;
                    return View("Index", ratings);
                }

                if (Mark < 0 || Mark > 100)
                {
                    ModelState.AddModelError("", "Rating must be between 0 and 100");
                    var ratings = await _ratingService.GetRatesAsync();
                    ViewBag.RatingService = _ratingService;
                    return View("Index", ratings);
                }

                var rating = new Rating { Name = Name, mark = Mark };
                await _ratingService.AddRateAsync(rating);
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding rating for user {Name} with mark {Mark}", Name, Mark);
                ModelState.AddModelError("", "Failed to add rating. Please try again.");
                var ratings = await _ratingService.GetRatesAsync();
                ViewBag.RatingService = _ratingService;
                return View("Index", ratings);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetRates()
        {
            try
            {
                await _ratingService.ResetRatesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting ratings");
                ModelState.AddModelError("", "Failed to reset ratings. Please try again.");
                var ratings = await _ratingService.GetRatesAsync();
                ViewBag.RatingService = _ratingService;
                return View("Index", ratings);
            }
        }
    }
}