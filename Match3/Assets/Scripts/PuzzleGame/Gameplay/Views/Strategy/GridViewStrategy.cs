using Cysharp.Threading.Tasks;
using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Gameplay.Helpers;
using PuzzleGame.Util;
using UnityEngine;

namespace PuzzleGame.Gameplay.Views.Strategy
{
    public abstract class GridViewStrategy
    {
        public readonly GridType GridType;
        protected readonly GridColor GridColor;
        
        protected GridViewStrategy(GridType gridType, GridColor gridColor)
        {
            GridType = gridType;
            GridColor = gridColor;
        }

        public abstract Sprite GetSprite(int health, GridState gridState);

        public virtual UniTask PlayCreationAnimation(Transform _)
        {
            // Do nothing by default
            return UniTask.CompletedTask;
        }

        public virtual async UniTask PlayBreakAnimation(Transform _, Vector3 position)
        {
            await PlayBreakAnimationWithSized(position, ParticleSize.Medium);
        }

        protected async UniTask PlayBreakAnimationWithSized(Vector3 position, ParticleSize size)
        {
            var spawnedParticle = GridParticleProvider.Instance.GetRandomizedParticleSystem(GridType, GridColor);
            spawnedParticle.transform.position = position;
            spawnedParticle.Play(size);
            await spawnedParticle.WaitForStop(GameplayVariables.Instance.LifetimeToken);
            GridParticleProvider.Instance.ReturnRandomizedParticleSystem(GridType, GridColor, spawnedParticle);
        }
        
        public abstract UniTask PlaySuperBreakAnimation(Transform zeroPointPosition,  Vector3 position,
            GridViewStrategy otherBrokenGrid);
    }
}