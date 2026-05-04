using System.Collections.Generic;
using Data;

public static class GameContext
{
    public enum GameMode
    {
        Poetry,
        Lyrics
    }

    public static GameMode Mode;
    public static string TopicName;

    public static int Score;

    public static KeywordChecker KeywordChecker;
}