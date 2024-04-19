using PuzzleGame.Player;
using PuzzleGame.Player.Abstractions;
using UnityEngine;

namespace PuzzleGame.Project
{
    [DefaultExecutionOrder(-5000)]
    public class ProjectContext : MonoBehaviour
    {
        [SerializeField] private int maxLevelCount = 10;
        
        public static ProjectContext Instance { get; private set; }
        
        public IPlayerRepositoryService PlayerRepositoryService { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            // Otherwise, set this instance as the singleton instance.
            Instance = this;
            DontDestroyOnLoad(this);

            PlayerRepositoryService = new PlayerRepositoryService(maxLevelCount);
        }
    }
}