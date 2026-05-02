using System.Linq;
using Nestor.Poetry;

public class Poetry
{
    private static Poetry _instance;
    public static Poetry Instance => _instance ??= new();

    private readonly RhymeAnalyzer _analyzer = new();

    public float ScoreRhyme(string line1, string line2)
    {
        RhymingPair pair = _analyzer.ScoreRhyme(line1.Split(' ').Last(), line2.Split(' ').Last());
        return (float)pair.Score;
    }
}