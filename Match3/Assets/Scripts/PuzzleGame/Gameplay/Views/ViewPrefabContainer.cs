using System.Collections.Generic;
using PuzzleGame.Gameplay.Context;
using PuzzleGame.Gameplay.DataStructures;

namespace PuzzleGame.Gameplay.Views
{
    public class ViewPrefabContainer
    {
        private readonly Dictionary<GridType, GridView> _gridViewPrefabDictionary = new ();

        public GridView GetGridViewPrefab(GridType gridType)
        {
            if (_gridViewPrefabDictionary.TryGetValue(gridType, out var prefab))
            {
                return prefab;
            }

            var gridView = GameplayAssetProvider.Instance.GetGridPrefab(gridType).GetComponent<GridView>();
            _gridViewPrefabDictionary.Add(gridType, gridView);
            return gridView;
        }
    }
}