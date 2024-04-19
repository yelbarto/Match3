using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Gameplay.Models.Strategy;

namespace PuzzleGame.Gameplay.Models
{
    public class ObstacleModel : GridModel
    {
        public ObstacleModel(GridType gridType, int id, ObstacleBreakableModelStrategy strategy, int health)
            : base(gridType, id, strategy, health)
        {
            IsObstacle = true;
        }

        public override bool CanFall()
        {
            return GridType is GridType.Vase;
        }
    }
}