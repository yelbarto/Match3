using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PuzzleGame.Gameplay.Context;
using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Gameplay.Helpers;
using PuzzleGame.Gameplay.Views.Strategy;
using PuzzleGame.Util;
using PuzzleGame.Util.Pool;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace PuzzleGame.Gameplay.Views
{
    public class GridView : MonoBehaviour, IPoolable
    {
        private const string DROP = "Drop Parameters";
        private const string EXPLODE = "Explode Parameters";
        private const string REF = "References";

        [SerializeField, FoldoutGroup(REF)] private SpriteRenderer itemImage;
        [SerializeField, FoldoutGroup(REF)] private GameObjectInputComponent inputComponent;

        [SerializeField, FoldoutGroup(DROP)] private AnimationCurve dropEase;
        [SerializeField, FoldoutGroup(DROP)] private AnimationCurve dropSpeedEase;
        [SerializeField, FoldoutGroup(DROP)] private float minSpeedValue = 3f;
        [SerializeField, FoldoutGroup(DROP)] private float maxSpeedValue = 20f;
        [SerializeField, FoldoutGroup(DROP)] private float dropOffsetMultiplier = 0.03f;


        public event Action OnGridClicked;

        private Transform _transform;
        private GridType _gridType;
        private GridColor _gridColor;
        private GridViewStrategy _gridViewStrategy;
        private CancellationTokenSource _dropCts;
        private CancellationTokenSource _spawnCts;
        private CancellationTokenSource _lifetimeCts;

        private void Awake()
        {
            _transform = transform;
            inputComponent.OnObjectClicked += () => OnGridClicked?.Invoke();
            _lifetimeCts = new CancellationTokenSource();
        }

        public void SetUp(GridType gridType, bool interactable, Vector2Int position,
            GridViewStrategy gridViewStrategy, GridColor gridColor)
        {
            _gridViewStrategy = gridViewStrategy;
            SetCommonValues(gridType, position);
            SetInteractable(interactable);
            _gridColor = gridColor;
            if (gridType is GridType.Vase or GridType.Box or GridType.Stone)
                _spawnCts = new CancellationTokenSource();
        }

        private void SetCommonValues(GridType gridType, Vector2Int position)
        {
            _gridType = gridType;
            SetPosition(position);
        }

        private static Vector3 GetPosition(Vector2Int position)
        {
            return new Vector3(position.x, position.y, -position.y / 10f);
        }

        private void SetPosition(Vector2Int position)
        {
            _transform.localPosition = GetPosition(position);
        }

        public void SetSprite(int health, GridState state)
        {
            var currentSprite = _gridViewStrategy.GetSprite(health, state);
            if (itemImage.sprite != currentSprite)
                itemImage.sprite = currentSprite;
        }
        
        public async UniTask CrackGridAsync(GridViewStrategy strategy, bool isCompletelyCracked)
        {
            if (isCompletelyCracked)
                gameObject.SetActive(false);
            if (strategy != null)
                await _gridViewStrategy.PlaySuperBreakAnimation(_transform, _transform.position, strategy);
            else
                await _gridViewStrategy.PlayBreakAnimation(_transform, _transform.position);
        }

        public async UniTask DropGridSpeedBaseAsync(Vector2Int position, int belowGridExplodeOffset,
            int dropOffset)
        {
            _dropCts?.Cancel();
            _dropCts = new CancellationTokenSource();
            await UniTask.Delay(
                TimeSpan.FromSeconds(GameplayVariables.Instance.DropWaitForScaleDuration
                                     + GameplayVariables.Instance.ExplodeOffsetMultiplier * belowGridExplodeOffset
                                     + dropOffset * dropOffsetMultiplier), 
                cancellationToken: _dropCts.Token);

            _transform.DOKill();
            var diffY = _transform.localPosition.y - position.y;
            var speed = diffY >= 6 ? diffY * 2 : diffY >= 4 ? diffY * 2.5f : diffY * 3;
            speed = Mathf.Clamp(speed, minSpeedValue, maxSpeedValue);
            _transform.DOLocalMove(GetPosition(position), speed).SetEase(dropSpeedEase)
                .SetSpeedBased().ToUniTask(TweenCancelBehaviour.CompleteAndCancelAwait, _dropCts.Token).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(diffY / speed - GameplayVariables.Instance.DropAnimationFinalOffset),
                cancellationToken: _dropCts.Token);
        }

        public void SetInteractable(bool interactable)
        {
            inputComponent.SetInteractable(interactable);
        }

        private void OnDestroy()
        {
            _spawnCts?.Cancel();
            _lifetimeCts?.Cancel();
        }

        public void OnDespawn()
        {
            itemImage.sprite = null;
            inputComponent.SetInteractable(false);
            gameObject.SetActive(false);
            _spawnCts?.Cancel();
            _dropCts?.Cancel();
        }

        public void OnSpawn()
        {
            _spawnCts?.Cancel();
            transform.localScale = Vector3.one;
            _spawnCts = new CancellationTokenSource();
            gameObject.SetActive(true);
        }

        #region Debug

        [Button]
        public void CrackAnimationTest()
        {
            _transform.localScale = Vector3.one;
            CrackGridAsync(null, true).Forget();
        }
        
        [Button]
        public void DropGridTest(Vector2Int initialPosition, Vector2Int endPosition, float speed)
        {
            SetPosition(initialPosition);
            _transform.DOLocalMove(GetPosition(endPosition), speed).SetEase(dropEase).SetSpeedBased();
        }

        #endregion
    }
}