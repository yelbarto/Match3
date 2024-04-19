using PuzzleGame.Gameplay.Views;
using PuzzleGame.Util.Pool;
using UnityEngine;

namespace PuzzleGame.Gameplay.Presenters
{
    public class GridPresenterFactory
    {
        private readonly GameObjectPool<GridView> _cubePool;
        private readonly ViewPrefabContainer _viewPrefabContainer;

        public GridPresenterFactory(Transform gridParent, GridView cubePrefab)
        {
            _cubePool = new GameObjectPool<GridView>(cubePrefab, gridParent);
            _viewPrefabContainer = new ViewPrefabContainer();
            _cubePool.LoadPrefab(100);
        }
        
        public GridPresenter Create()
        {
            return new GridPresenter(_cubePool, _viewPrefabContainer);
        }
    }
}