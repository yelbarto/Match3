namespace PuzzleGame.Gameplay.Models.Strategy
{
    public class ObstacleBreakableModelStrategy : BreakableModelStrategy
    {
        private readonly bool _isOnlyBreakableWithSpecialItem;

        public ObstacleBreakableModelStrategy(bool isOnlyBreakableWithSpecialItem)
        {
            _isOnlyBreakableWithSpecialItem = isOnlyBreakableWithSpecialItem;
        }
        
        public override bool TryBreak(bool isSpecialMatch)
        {
            if (_isOnlyBreakableWithSpecialItem && !isSpecialMatch)
            {
                return false;
            }
            return true;
        }
    }
}