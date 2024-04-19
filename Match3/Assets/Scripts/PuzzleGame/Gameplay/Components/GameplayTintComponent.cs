using UnityEngine;

namespace PuzzleGame.Gameplay.Components
{
    public class GameplayTintComponent : MonoBehaviour
    {
        public static GameplayTintComponent Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            Close();
        }
        
        public void Open()
        {
            gameObject.SetActive(true);
        }
        
        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}