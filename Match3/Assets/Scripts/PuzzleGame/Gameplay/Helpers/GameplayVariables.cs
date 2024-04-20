using UnityEngine;

namespace PuzzleGame.Gameplay.Context
{
    public class GameplayVariables : MonoBehaviour
    {
        [SerializeField] private int creationHeightOffset = 5;
        [SerializeField] private Transform bottomLeftRocketPositioner;
        [SerializeField] private Transform topRightRocketPositioner;
        
        public int CreationHeightOffset => creationHeightOffset;
        public Transform BottomLeftRocketPositioner => bottomLeftRocketPositioner;
        public Transform TopRightRocketPositioner => topRightRocketPositioner;
        
        public static GameplayVariables Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
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
                Instance = null;
            }
        }
    }
}