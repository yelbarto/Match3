using System;
using PuzzleGame.Data.DataStructures;
using UnityEngine;

namespace PuzzleGame.Data.Services
{
    public class LevelRepositoryService
    {
        public LevelData LoadLevel(int level)
        {
            var levelFileName = $"level_{level:00}";
            var loadedData = Resources.Load<TextAsset>(levelFileName);
            if (loadedData is null)
                throw new Exception("Level not found " + levelFileName);
            return JsonUtility.FromJson<LevelData>(loadedData.text);
        }
    }
}