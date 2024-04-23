using System.Threading;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace PuzzleGame.Gameplay.Helpers
{
    public class GameplayVariables : MonoBehaviour
    {
        [FormerlySerializedAs("creationHeightOffset")] [SerializeField] private int dropHeightOffset = 5;
        [SerializeField] private Transform bottomLeftRocketPositioner;
        [SerializeField] private Transform topRightRocketPositioner;
        [SerializeField] private int rocketInitialExplodeDistance = 1;
        
        [SerializeField] private float explodeOffsetMultiplier = 0.03f;
        [SerializeField] private float dropAnimationFinalOffset = 0.05f;
        [SerializeField] private float cubeStateChangeWaitDuration = 0.05f;
        
        [SerializeField] private float cubeScaleDownDuration = 0.2f;
        [SerializeField] private float cubeScaleDownWaitDuration = 0.15f;
        [SerializeField] private float dropWaitForScaleDuration = 0.15f;
        [SerializeField] private Ease cubeScaleDownEase = Ease.OutCubic;
        
        [SerializeField] private float gridCreationScaleUpValue = 1.1f;
        [SerializeField] private float gridCreationScaleUpDuration = 0.2f;
        [SerializeField] private Ease gridCreationScaleUpEase = Ease.InSine;
        
        public int DropHeightOffset => dropHeightOffset;
        public float DropAnimationFinalOffset => dropAnimationFinalOffset;
        public int RocketInitialExplodeDistance => rocketInitialExplodeDistance;
        public float ExplodeOffsetMultiplier => explodeOffsetMultiplier;
        public float CubeStateChangeWaitDuration => cubeStateChangeWaitDuration;
        public float GridCreationScaleUpValue => gridCreationScaleUpValue;
        public float GridCreationScaleUpDuration => gridCreationScaleUpDuration;
        public Ease GridCreationScaleUpEase => gridCreationScaleUpEase;
        public Transform BottomLeftRocketPositioner => bottomLeftRocketPositioner;
        public Transform TopRightRocketPositioner => topRightRocketPositioner;
        public float CubeScaleDownDuration => cubeScaleDownDuration;
        public float CubeScaleDownWaitDuration => cubeScaleDownWaitDuration;
        public float DropWaitForScaleDuration => dropWaitForScaleDuration;
        public Ease CubeScaleDownEase => cubeScaleDownEase;
        public CancellationToken LifetimeToken => _lifetimeCts.Token;
        
        public static GameplayVariables Instance { get; private set; }
        
        private CancellationTokenSource _lifetimeCts;
        
        private void Awake()
        {
            if (Instance == null)
            {
                _lifetimeCts = new CancellationTokenSource();
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                _lifetimeCts?.Cancel();
                Instance = null;
            }
        }
    }
}