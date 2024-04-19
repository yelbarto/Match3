using PuzzleGame.Player.Abstractions;
using UnityEngine;

namespace PuzzleGame.Player
{
    public class PlayerRepositoryService : IPlayerRepositoryService
    {
        private readonly int _maxLevelCount;
        public int CurrentLevel { get; private set; } = PlayerPrefs.GetInt(LEVEL_PLAYER_PREFS_KEY, 1);

        private const string LEVEL_PLAYER_PREFS_KEY = "CurrentLevel";

        public PlayerRepositoryService(int maxLevelCount)
        {
            _maxLevelCount = maxLevelCount;
            if (CurrentLevel > maxLevelCount)
                LevelCompleted();
        }

        public void LevelCompleted()
        {
            if (CurrentLevel >= _maxLevelCount)
                CurrentLevel = -1;
            else
                CurrentLevel++;
            PlayerPrefs.SetInt(LEVEL_PLAYER_PREFS_KEY, CurrentLevel);
        }

        public bool ChangeLevel(int level)
        {
            if (CurrentLevel >= _maxLevelCount || level < 1)
                return false;
            CurrentLevel = level;
            PlayerPrefs.SetInt(LEVEL_PLAYER_PREFS_KEY, CurrentLevel);
            return true;
        }
    }
}