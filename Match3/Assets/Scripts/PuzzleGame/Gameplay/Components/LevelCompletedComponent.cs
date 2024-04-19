using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Gameplay.Components
{
    public class LevelCompletedComponent : MonoBehaviour
    {
        private const string FIRST_ANIM = "First Animation";
        private const string SECOND_ANIM = "Second Animation";
        private const string THIRD_ANIM = "Third Animation";
        private const string COMMON_ANIM = "Common Animation";
        
        [SerializeField, FoldoutGroup(FIRST_ANIM)] private Transform imageAnimationTransform;
        [SerializeField, FoldoutGroup(FIRST_ANIM)] private float firstAnimationParticleDelay;
        [SerializeField, FoldoutGroup(FIRST_ANIM)] private float firstAnimationDelay;
        [SerializeField, FoldoutGroup(FIRST_ANIM)] private ParticleSystem starParticleSystem;
        [SerializeField, FoldoutGroup(SECOND_ANIM)] private Transform textAnimationTransform;
        [SerializeField, FoldoutGroup(SECOND_ANIM)] private float secondAnimationDelay;
        [SerializeField, FoldoutGroup(COMMON_ANIM)] private float openAnimationDuration;
        [SerializeField, FoldoutGroup(COMMON_ANIM)] private Ease openAnimationEase;
        [SerializeField, FoldoutGroup(THIRD_ANIM)] private TMP_Text tapToContinueText;
        [SerializeField, FoldoutGroup(THIRD_ANIM)] private float tapToContinueFadeDuration;
        [SerializeField, FoldoutGroup(THIRD_ANIM)] private Ease tapToContinueFadeEase;
        [SerializeField] private Button closeButton;
        
        private CancellationTokenSource _openCts;
        private Action _closeCallback;

        private void Awake()
        {
            closeButton.onClick.AddListener(Close);
            gameObject.SetActive(false);
        }

        [Button]
        public void Open(Action closeAction)
        {
            GameplayTintComponent.Instance.Open();
            imageAnimationTransform.localScale = Vector3.zero;
            textAnimationTransform.localScale = Vector3.zero;
            tapToContinueText.DOFade(0, 0);
            _closeCallback = closeAction;
            closeButton.interactable = false;
            starParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            PlayOpenAnimationAsync().Forget();
        }

        private async UniTask PlayOpenAnimationAsync()
        {
            _openCts?.Cancel();
            _openCts = new CancellationTokenSource();
            //TODO: Add token for DOTween
            gameObject.SetActive(true);
            imageAnimationTransform.DOScale(1, openAnimationDuration).SetEase(openAnimationEase);
            await UniTask.Delay(TimeSpan.FromSeconds(firstAnimationParticleDelay), cancellationToken: _openCts.Token);
            starParticleSystem.Play();
            await UniTask.Delay(TimeSpan.FromSeconds(firstAnimationDelay), cancellationToken: _openCts.Token);
            textAnimationTransform.DOScale(1, openAnimationDuration).SetEase(openAnimationEase);
            await UniTask.Delay(TimeSpan.FromSeconds(secondAnimationDelay), cancellationToken: _openCts.Token);
            tapToContinueText.DOFade(1, tapToContinueFadeDuration).SetEase(tapToContinueFadeEase);
            closeButton.interactable = true;
        }

        private void Close()
        {
            closeButton.interactable = false;
            _closeCallback?.Invoke();
            _closeCallback = null;
            // GameplayTintComponent.Instance.Close();
            // _openCts?.Cancel();
            // gameObject.SetActive(false);
        }
    }
}