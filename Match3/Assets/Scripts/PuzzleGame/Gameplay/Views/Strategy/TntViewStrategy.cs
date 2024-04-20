using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Gameplay.Helpers;
using PuzzleGame.Util;
using UnityEngine;

namespace PuzzleGame.Gameplay.Views.Strategy
{
    public class TntViewStrategy : SpecialItemViewStrategy
    {
        private readonly RocketViewStrategy _horizontalRocketViewStrategy;
        private readonly RocketViewStrategy _verticalRocketViewStrategy;
        private readonly Vector2Int _border;

        public TntViewStrategy(Vector2Int border, RocketViewStrategy verticalRocketViewStrategy,
            RocketViewStrategy horizontalRocketViewStrategy) : base(GridType.Tnt)
        {
            _horizontalRocketViewStrategy = horizontalRocketViewStrategy;
            _verticalRocketViewStrategy = verticalRocketViewStrategy;
            _border = border;
        }

        public override async UniTask PlaySuperBreakAnimation(Transform zeroPoint, Vector3 position, 
            GridViewStrategy otherBrokenGrid)
        {
            if (otherBrokenGrid.GridType == GridType.Tnt)
            {
                await PlayBreakAnimationWithSized(position, ParticleSize.Large);
            }
            else
            {
                var animationTaskList = new List<UniTask> { PlayBreakAnimationWithSized(position, ParticleSize.Small) };
                var localZeroPoint = zeroPoint.localPosition;
                var intZeroPoint = new Vector2Int((int)localZeroPoint.x, (int)localZeroPoint.y);
                var effectedArea = EffectedAreaCalculator.GetAdjacentPositions(_border,
                    intZeroPoint);
                PlayTntAnimations(zeroPoint, position, otherBrokenGrid, effectedArea, intZeroPoint, animationTaskList);

                await animationTaskList;
            }
        }

        private void PlayTntAnimations(Transform zeroPoint, Vector3 position, GridViewStrategy otherBrokenGrid,
            Vector2Int[] effectedArea, Vector2Int intZeroPoint, List<UniTask> animationTaskList)
        {
            foreach (var area in effectedArea)
            {
                var xDiff = area.x - intZeroPoint.x;
                if (xDiff != 0)
                {
                    var animationPosition = new Vector3(position.x + xDiff * zeroPoint.lossyScale.x, 
                        position.y, position.z);
                    animationTaskList.Add(_verticalRocketViewStrategy.PlayBreakAnimation(zeroPoint, 
                        animationPosition));
                }
                else
                {
                    var yDiff = area.y - intZeroPoint.y;
                    if (yDiff != 0)
                    {
                        var animationPosition = new Vector3(position.x, 
                            position.y + yDiff * zeroPoint.lossyScale.y, position.z);
                        animationTaskList.Add(_horizontalRocketViewStrategy.PlayBreakAnimation(zeroPoint,
                            animationPosition));
                    }
                    else
                    {
                        animationTaskList.Add(_horizontalRocketViewStrategy.PlaySuperBreakAnimation(zeroPoint,
                            position, _verticalRocketViewStrategy));
                    }
                }
            }
        }
    }
}