using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PuzzleGame.Gameplay.Context;
using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Gameplay.Helpers;
using UnityEngine;

namespace PuzzleGame.Gameplay.Views.Strategy
{
    public class CubeViewStrategy : GridViewStrategy
    {
        private readonly Transform _scaleTransform;

        public CubeViewStrategy(Transform scaleTransform, GridColor gridColor) : base(GridType.Cube, gridColor)
        {
            _scaleTransform = scaleTransform;
        }
        
        public override Sprite GetSprite(int health, GridState gridState)
        {
            return GameplayAssetProvider.Instance.GetGridSprite(GridColor, gridState);
        }

        public override async UniTask PlayBreakAnimation(Transform zeroPointPosition, Vector3 position)
        {
            _scaleTransform.gameObject.SetActive(true);
            _scaleTransform.DOScale(0, GameplayVariables.Instance.CubeScaleDownDuration)
                .SetEase(GameplayVariables.Instance.CubeScaleDownEase);
            await UniTask.Delay(TimeSpan.FromSeconds(GameplayVariables.Instance.CubeScaleDownWaitDuration),
                cancellationToken: GameplayVariables.Instance.LifetimeToken);
            await base.PlayBreakAnimation(zeroPointPosition, position);
        }

        public override async UniTask PlaySuperBreakAnimation(Transform zeroPoint, Vector3 position,
            GridViewStrategy otherBrokenGrid)
        {
            await PlayBreakAnimation(zeroPoint, position);
        }
    }
}