namespace PuzzleGame.Gameplay.Models.Strategy
{
    public abstract class BreakableModelStrategy
    {
        public abstract bool TryBreak(bool isSpecialMatch);
    }
}