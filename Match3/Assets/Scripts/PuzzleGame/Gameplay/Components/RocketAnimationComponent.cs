using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PuzzleGame.Gameplay.DataStructures;
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
        [SerializeField] private float rocketDuration = 2f;
        [SerializeField] private Ease rocketEase = Ease.OutSine;

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


            firstRocket.DOMove(firstRocketPosition, rocketSpeed).SetEase(rocketEase).SetSpeedBased()
                .ToUniTask(TweenCancelBehaviour.CompleteAndCancelAwait, _animationCts.Token).Forget();
            secondRocket.DOMove(secondRocketPosition, rocketSpeed).SetEase(rocketEase).SetSpeedBased()
                .ToUniTask(TweenCancelBehaviour.CompleteAndCancelAwait, _animationCts.Token).Forget();
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
            await UniTask.Delay(TimeSpan.FromSeconds(rocketDuration), cancellationToken: token);
            CleanUp();
        }

        private void OnDestroy()
        {
            _animationCts?.Cancel();
        }
    }
}