
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Datas.Entity;
using Datas.Service;
using Datas.Service.Scores;
using Microsoft.Extensions.Logging;

public class ScoreServiceEF : IScoreService
    {
        private readonly SlideALamaDbContext _context;
        private readonly ILogger<ScoreServiceEF> _logger;

        public ScoreServiceEF(SlideALamaDbContext context, ILogger<ScoreServiceEF> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddScoreAsync(Score score)
        {
            try
            {
                if (score == null)
                    throw new ArgumentNullException(nameof(score));

                if (string.IsNullOrWhiteSpace(score.Player))
                    throw new ArgumentException("Player name cannot be empty", nameof(score));

                if (score.Points < 0)
                    throw new ArgumentException("Points cannot be negative", nameof(score));

                _context.Scores.Add(score);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Score added for player {Player} with {Points} points", 
                    score.Player, score.Points);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while adding score for player {Player}", score?.Player);
                throw new InvalidOperationException("Failed to save score to database", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding score for player {Player}", score?.Player);
                throw;
            }
        }

        public async Task<IList<Score>> GetTopScoresAsync(int count = 10)
        {
            try
            {
                var scores = await _context.Scores
                    .OrderByDescending(s => s.Points)
                    .Take(count)
                    .ToListAsync();
                
                _logger.LogInformation("Retrieved {Count} top scores", scores.Count);
                return scores;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top scores");
                throw new InvalidOperationException("Failed to retrieve top scores", ex);
            }
        }

        public async Task ResetScoreAsync()
        {
            try
            {
                var allScores = await _context.Scores.ToListAsync();
                _context.Scores.RemoveRange(allScores);
                await _context.SaveChangesAsync();
                
                _logger.LogWarning("All scores have been reset");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting scores");
                throw new InvalidOperationException("Failed to reset scores", ex);
            }
        }

        public void AddScore(Score score)
        {
            AddScoreAsync(score).GetAwaiter().GetResult();
        }

        public IList<Score> GetTopScores()
        {
            return GetTopScoresAsync().GetAwaiter().GetResult();
        }

        public void ResetScore()
        {
            ResetScoreAsync().GetAwaiter().GetResult();
        }
    }




