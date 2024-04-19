namespace PuzzleGame.Player.Abstractions
{
    public interface IPlayerRepositoryService
    {
        int CurrentLevel { get; }
        void LevelCompleted();
        bool ChangeLevel(int level);
    }
}