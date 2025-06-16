
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Datas.Entity;
using Datas.Service.Rates;

namespace Datas.Service.Rates
{
    public class RatingServiceEF : IRatingService
    {
        private readonly SlideALamaDbContext _context;
        private readonly ILogger<RatingServiceEF> _logger;

        public RatingServiceEF(SlideALamaDbContext context, ILogger<RatingServiceEF> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddRateAsync(Rating rating)
        {
            try
            {
                if (rating == null)
                    throw new ArgumentNullException(nameof(rating));

                if (string.IsNullOrWhiteSpace(rating.Name))
                    throw new ArgumentException("Rating name cannot be empty", nameof(rating));

                if (rating.mark < 0 || rating.mark > 100)
                    throw new ArgumentException("Rating mark must be between 0 and 100", nameof(rating));

                _context.Rating.Add(rating);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Rating added for user {Name} with mark {Mark}", 
                    rating.Name, rating.mark);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while adding rating for user {Name}", rating?.Name);
                throw new InvalidOperationException("Failed to save rating to database", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding rating for user {Name}", rating?.Name);
                throw;
            }
        }

        public async Task<IList<Rating>> GetRatesAsync()
        {
            try
            {
                var ratings = await _context.Rating
                    .OrderByDescending(r => r.mark)
                    .ToListAsync();
                
                _logger.LogInformation("Retrieved {Count} ratings", ratings.Count);
                return ratings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ratings");
                throw new InvalidOperationException("Failed to retrieve ratings", ex);
            }
        }

        public async Task ResetRatesAsync()
        {
            try
            {
                var allRatings = await _context.Rating.ToListAsync();
                _context.Rating.RemoveRange(allRatings);
                await _context.SaveChangesAsync();
                
                _logger.LogWarning("All ratings have been reset");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting ratings");
                throw new InvalidOperationException("Failed to reset ratings", ex);
            }
        }

        // Synchronous methods for backwards compatibility
        public void AddRate(Rating rating)
        {
            AddRateAsync(rating).GetAwaiter().GetResult();
        }

        public IList<Rating> GetRates()
        {
            return GetRatesAsync().GetAwaiter().GetResult();
        }

        public void ResetRates()
        {
            ResetRatesAsync().GetAwaiter().GetResult();
        }
    }
}

