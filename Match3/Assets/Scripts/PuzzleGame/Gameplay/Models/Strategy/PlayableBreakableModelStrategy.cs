namespace PuzzleGame.Gameplay.Models.Strategy
{
    public class PlayableBreakableModelStrategy : BreakableModelStrategy
    {
        public override bool TryBreak(bool isSpecialMatch)
        {
            return true;
        }
    }
}