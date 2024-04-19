using System;
using PuzzleGame.MainScene.Presenters;
using PuzzleGame.Project;
using UnityEngine;

namespace PuzzleGame.MainScene.Context
{
    public class MainSceneContext : MonoBehaviour
    {
        [SerializeField] private MainSceneView mainSceneView;
        
        private MainScenePresenter _mainScenePresenter;
        
        public void Awake()
        {
            _mainScenePresenter = new MainScenePresenter(mainSceneView, ProjectContext.Instance.PlayerRepositoryService);
            _mainScenePresenter.Initialize();
        }

        private void OnDestroy()
        {
            _mainScenePresenter.Dispose();
        }
    }
}