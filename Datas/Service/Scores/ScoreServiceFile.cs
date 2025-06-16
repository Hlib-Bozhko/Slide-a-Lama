
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Datas.Entity;
using Datas.Service.Scores;

public class ScoreServiceFile : IScoreService
{
    private const string FileName = "score.bin";

    private IList<Score> _scores = new List<Score>();
    public async Task AddScoreAsync(Score score)
    {
        throw new System.NotImplementedException();
    }

    public async Task<IList<Score>> GetTopScoresAsync(int count = 10)
    {
        throw new System.NotImplementedException();
    }

    public async Task ResetScoreAsync()
    {
        throw new System.NotImplementedException();
    }

    void IScoreService.AddScore(Score score)
    {
        _scores.Add(score);
        SaveScores();
    }

    IList<Score> IScoreService.GetTopScores()
    {
        LoadScores();
        return (from s in _scores orderby s.Points descending select s).Take(5).ToList();
    }

    void IScoreService.ResetScore()
    {
        _scores.Clear();
        File.Delete(FileName);
    }

    private void SaveScores()
    {
        using (var fs = File.OpenWrite(FileName))
        {
            var bf = new BinaryFormatter();
            bf.Serialize(fs, _scores);
        }
    }

    private void LoadScores()
    {
        if (File.Exists(FileName))
        {
            using (var fs = File.OpenRead(FileName))
            {
                var bf = new BinaryFormatter();
                _scores = (List<Score>)bf.Deserialize(fs);
            }
        }
    }
}

