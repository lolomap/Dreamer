using System.Linq;
using Nestor.Poetry;

public class PoetryLogic
{
    private static PoetryLogic _instance;
    public static PoetryLogic Instance => _instance ??= new();

    private readonly RhymeAnalyzer _analyzer = new();

    public float ScoreRhyme(string line1, string line2)
    {
        RhymingPair pair = _analyzer.ScoreRhyme(line1.Split(' ').Last(), line2.Split(' ').Last());
        return (float)pair.Score;
    }
}