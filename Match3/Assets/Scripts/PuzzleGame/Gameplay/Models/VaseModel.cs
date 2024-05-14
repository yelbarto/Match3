using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Gameplay.Models.Strategy;

namespace PuzzleGame.Gameplay.Models
{
    public class VaseModel : ObstacleModel
    {
        public VaseModel(GridType gridType, int id, ObstacleBreakableModelStrategy strategy, int health) : base(
            gridType, id, strategy, health)
        {
        }

        public override bool CanFall()
        {
            return true;
        }
    }
}