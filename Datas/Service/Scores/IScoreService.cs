
using System.Collections.Generic;
using System.Threading.Tasks;
using Datas.Entity;

namespace Datas.Service.Scores
{
    public interface IScoreService
    {
        // Async methods
        Task AddScoreAsync(Score score);
        Task<IList<Score>> GetTopScoresAsync(int count = 10);
        Task ResetScoreAsync();

        // Standard methods
        void AddScore(Score score);
        IList<Score> GetTopScores();
        void ResetScore();
    }
}

