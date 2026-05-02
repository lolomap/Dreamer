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
}