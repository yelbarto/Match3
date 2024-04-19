using PuzzleGame.Gameplay.Presenters;
using PuzzleGame.Gameplay.Views;
using PuzzleGame.Project;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PuzzleGame.Gameplay.Context
{
    public class GameplayContext : MonoBehaviour
    {
        private const string DI = "Dependency Injection";
        private const string REF = "References";
        
        [SerializeField, FoldoutGroup(DI)] private LevelView levelView;
        [SerializeField, FoldoutGroup(DI)] private LevelFailView levelFailView;
        [SerializeField, FoldoutGroup(DI)] private DebugView debugView;
        
        [SerializeField, FoldoutGroup(REF)] private Transform gridParent;
        [SerializeField, FoldoutGroup(REF)] private GridView gridPrefab;
        
        private LevelPresenter _levelPresenter;

        private void Awake()
        {
            _levelPresenter = new LevelPresenter(ProjectContext.Instance.PlayerRepositoryService, levelView,
                gridParent, gridPrefab, levelFailView, debugView);
            _levelPresenter.Initialize();
        }
    }
}