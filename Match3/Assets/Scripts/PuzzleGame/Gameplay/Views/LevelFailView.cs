using System;
using DG.Tweening;
using PuzzleGame.Gameplay.Components;
using PuzzleGame.Util.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PuzzleGame.Gameplay.Views
{
    public class LevelFailView : MonoBehaviour
    {
        [SerializeField] private PersonalizedButton retryButton;
        [SerializeField] private PersonalizedButton closeButton;
        [SerializeField] private float scaleAmount;
        [SerializeField] private float scaleDuration;
        [SerializeField] private Ease scaleEase;

        private Action<bool> _closedCallback;
        private Transform _animationTransform;

        private void Awake()
        {
            retryButton.onClick.AddListener(() => Close(true));
            closeButton.onClick.AddListener(() => Close(false));
            _animationTransform = transform;
            gameObject.SetActive(false);
        }

        [Button]
        public void Open(Action<bool> closeCallback)
        {
            _closedCallback = closeCallback;
            GameplayTintComponent.Instance.Open();
            _animationTransform.localScale = Vector3.one;
            gameObject.SetActive(true);
            _animationTransform.DOScale(scaleAmount, scaleDuration).SetEase(scaleEase)
                .SetLoops(2, LoopType.Yoyo);
        }

        private void Close(bool isRetry)
        {
            _closedCallback?.Invoke(isRetry);
            _closedCallback = null;
            if (!isRetry) return;
            GameplayTintComponent.Instance.Close();
            _animationTransform.DOKill();
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            retryButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();
        }
    }
}