using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Util;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PuzzleGame.Gameplay.Components
{
    public class RocketAnimationComponent : MonoBehaviour
    {
        [SerializeField] private Transform firstRocket;
        [SerializeField] private Transform secondRocket;
        [SerializeField] private Direction direction;
        [SerializeField] private float rocketSpeed = 0.5f;
        [SerializeField] private Ease rocketEase = Ease.OutSine;
        [SerializeField] private RandomizedParticleSystem firstParticleSystem;
        [SerializeField] private RandomizedParticleSystem secondParticleSystem;

        private CancellationTokenSource _animationCts;

        [Button]
        public void PlayTest(Vector3 position, Vector3 topRightPosition, Vector3 bottomLeftPosition)
        {
            PlayAnimationAsync(position, topRightPosition, bottomLeftPosition).Forget();
        }

        public async UniTask PlayAnimationAsync(Vector3 position, Vector3 topRightPosition, Vector3 bottomLeftPosition)
        {
            _animationCts?.Cancel();
            _animationCts = new CancellationTokenSource();
            transform.position = position;
            Vector3 firstRocketPosition;
            Vector3 secondRocketPosition;
            if (direction == Direction.Horizontal)
            {
                firstRocketPosition = new Vector3(bottomLeftPosition.x, position.y, position.z);
                secondRocketPosition = new Vector3(topRightPosition.x, position.y, position.z);
            }
            else
            {
                firstRocketPosition = new Vector3(position.x, topRightPosition.y, position.z);
                secondRocketPosition = new Vector3(position.x, bottomLeftPosition.y, position.z);
            }


            firstRocket.DOMove(firstRocketPosition, rocketSpeed).SetEase(rocketEase).SetSpeedBased();
            secondRocket.DOMove(secondRocketPosition, rocketSpeed).SetEase(rocketEase).SetSpeedBased();
            await CleanUpAsync(_animationCts.Token);
        }

        [Button]
        private void CleanUp()
        {
            firstRocket.DOKill();
            firstRocket.localPosition = Vector3.zero;
            secondRocket.DOKill();
            secondRocket.localPosition = Vector3.zero;
            gameObject.SetActive(false);
        }
        
        private async UniTask CleanUpAsync(CancellationToken token)
        {
            //TODO: Not working correct
            // await firstParticleSystem.WaitForStop(token);
            // await secondParticleSystem.WaitForStop(token);
            await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: token);
            CleanUp();
        }
    }
}