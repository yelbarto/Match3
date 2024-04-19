using UnityEngine;

namespace PuzzleGame.Gameplay.Context
{
    public class GameplayVariables : MonoBehaviour
    {
        [SerializeField] private int creationHeightOffset = 5;
        
        public int CreationHeightOffset => creationHeightOffset;
        
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