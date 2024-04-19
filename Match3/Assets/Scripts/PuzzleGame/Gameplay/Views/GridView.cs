using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Gameplay.Views.Strategy;
using PuzzleGame.Util;
using PuzzleGame.Util.Pool;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PuzzleGame.Gameplay.Views
{
    public class GridView : MonoBehaviour, IPoolable
    {
        [SerializeField] protected SpriteRenderer itemImage;
        [SerializeField] protected GameObjectInputComponent inputComponent;

        [SerializeField] private float dropDuration;
        [SerializeField] private AnimationCurve dropEase;
        [SerializeField] private AnimationCurve dropSpeedEase;
        [SerializeField] private float minSpeedValue = 3f;
        [SerializeField] private float maxSpeedValue = 20f;
        [SerializeField] private float dropMoveMaxDif = 3f;
        [SerializeField] private float dropMoveMinDif = 2f;
        [SerializeField] private float dropOffset = 0.03f;
        

        public event Action OnGridClicked;

        private Transform _transform;
        private GridType _gridType;
        private GridViewStrategy _gridViewStrategy;
        private int _currentDropOffset;
        private CancellationTokenSource _dropCts;

        protected virtual void Awake()
        {
            _transform = transform;
            inputComponent.OnObjectClicked += () => OnGridClicked?.Invoke();
        }

        public void SetUp(GridType gridType, bool interactable, Vector2Int position,
            GridViewStrategy gridViewStrategy, int dropOffset)
        {
            _gridViewStrategy = gridViewStrategy;
            SetCommonValues(gridType, position);
            SetInteractable(interactable);
            _currentDropOffset = dropOffset;
        }

        protected void SetCommonValues(GridType gridType, Vector2Int position)
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

        public void DropGridSpeedBase(Vector2Int position)
        {
            DropGridSpeedBaseAsync(position).Forget();
        }

        private async UniTask DropGridSpeedBaseAsync(Vector2Int position)
        {
            _dropCts?.Cancel();
            _dropCts = new CancellationTokenSource();
            if (_currentDropOffset != 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(dropOffset * _currentDropOffset),
                    cancellationToken: _dropCts.Token);
                _currentDropOffset = 0;
            }
            _transform.DOKill();
            var diffY = _transform.localPosition.y - position.y;
            var speed = diffY >= 6 ? diffY * 2 : diffY >= 4 ? diffY * 2.5f : diffY * 3;
            speed = Mathf.Clamp(speed, minSpeedValue, maxSpeedValue);
            _transform.DOLocalMove(GetPosition(position), speed).SetEase(dropSpeedEase)
                .SetSpeedBased();
        }

        public void DropGrid(Vector2Int position)
        {
            _transform.DOKill();
            var diffY = _transform.localPosition.y - position.y;
            if (diffY > dropMoveMaxDif)
                diffY = dropMoveMaxDif;
            else if (diffY < dropMoveMinDif)
                diffY = dropMoveMinDif;
            _transform.DOLocalMove(GetPosition(position), dropDuration * diffY).SetEase(dropEase);
        }

        [Button]
        public void DropGridTest(Vector2Int initialPosition, Vector2Int endPosition, bool isSpeed, float speed)
        {
            SetPosition(initialPosition);
            if (isSpeed)
            {
                _transform.DOLocalMove(GetPosition(endPosition), speed).SetEase(dropEase)
                    .SetSpeedBased();
            }
            else
                DropGrid(endPosition);
        }

        public void SetInteractable(bool interactable)
        {
            inputComponent.SetInteractable(interactable);
        }
        
        public void OnDespawn()
        {
            itemImage.sprite = null;
            inputComponent.SetInteractable(false);
            gameObject.SetActive(false);
            _dropCts?.Cancel();
        }

        public void OnSpawn()
        {
            gameObject.SetActive(true);
        }
    }
}