using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PuzzleGame.Gameplay.Context;
using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Gameplay.Helpers;
using UnityEngine;

namespace PuzzleGame.Gameplay.Views.Strategy
{
    public abstract class SpecialItemViewStrategy : GridViewStrategy
    {
        protected SpecialItemViewStrategy(GridType gridType) : base(gridType, GridColor.Default)
        {
        }

        public override async UniTask PlayCreationAnimation(Transform createdTransform)
        {
            var particleSystem = GridParticleProvider.Instance.GetCreationParticleSystem();
            var transform = particleSystem.transform;
            transform.parent = createdTransform;
            transform.localPosition = Vector3.zero;
            createdTransform.DOScale(GameplayVariables.Instance.GridCreationScaleUpValue,
                    GameplayVariables.Instance.GridCreationScaleUpDuration)
                .SetEase(GameplayVariables.Instance.GridCreationScaleUpEase)
                .SetLoops(2, LoopType.Yoyo);
            particleSystem.Play();
            await particleSystem.WaitForStop(GameplayVariables.Instance.LifetimeToken);
            GridParticleProvider.Instance.ReturnCreationParticleSystem(particleSystem);
        }

        public override Sprite GetSprite(int health, GridState state)
        {
            return GameplayAssetProvider.Instance.GetGridSprite(GridType);
        }
    }
}