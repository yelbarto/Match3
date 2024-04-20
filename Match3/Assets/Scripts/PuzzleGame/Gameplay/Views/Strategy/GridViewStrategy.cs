using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Gameplay.Helpers;
using PuzzleGame.Util;
using UnityEngine;

namespace PuzzleGame.Gameplay.Views.Strategy
{
    public abstract class GridViewStrategy : IDisposable
    {
        public readonly GridType GridType;
        protected readonly GridColor GridColor;
        
        //TODO: Cancel when everything is destroyed
        private readonly CancellationTokenSource _lifetimeCts;
        
        protected GridViewStrategy(GridType gridType, GridColor gridColor)
        {
            GridType = gridType;
            GridColor = gridColor;
            _lifetimeCts = new CancellationTokenSource();
        }

        public abstract Sprite GetSprite(int health, GridState gridState);

        public virtual async UniTask PlayBreakAnimation(Transform zeroPointPosition, Vector3 position)
        {
            await PlayBreakAnimationWithSized(position, ParticleSize.Medium);
        }

        protected async UniTask PlayBreakAnimationWithSized(Vector3 position, ParticleSize size)
        {
            var spawnedParticle = GridParticleProvider.Instance.GetRandomizedParticleSystem(GridType, GridColor);
            spawnedParticle.transform.position = position;
            spawnedParticle.Play(size);
            await spawnedParticle.WaitForStop(_lifetimeCts.Token);
            //TODO: RETURN LATER
            // GridParticleProvider.Instance.ReturnRandomizedParticleSystem(GridType, GridColor, spawnedParticle);
        }
        
        public abstract UniTask PlaySuperBreakAnimation(Transform zeroPointPosition,  Vector3 position,
            GridViewStrategy otherBrokenGrid);

        public void Dispose()
        {
            _lifetimeCts?.Cancel();
        }
    }
}