
using System.Collections.Generic;
using System.Threading.Tasks;
using Datas.Entity;

namespace Datas.Service.Rates
{
    public interface IRatingService
    {
        // Async methods
        Task AddRateAsync(Rating rating);
        Task<IList<Rating>> GetRatesAsync();
        Task ResetRatesAsync();

        // Standard methods (for backwards compatibility)
        void AddRate(Rating rating);
        IList<Rating> GetRates();
        void ResetRates();
    }
}

