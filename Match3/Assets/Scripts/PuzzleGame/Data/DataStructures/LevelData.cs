using System;
using UnityEngine;

namespace PuzzleGame.Data.DataStructures
{
    [Serializable]
    public class LevelData
    {
        [SerializeField] private int level_number;
        [SerializeField] private int grid_width;
        [SerializeField] private int grid_height;
        [SerializeField] private int move_count;
        [SerializeField] private string[] grid;
        
        public int LevelNumber => level_number;
        public int GridWidth => grid_width;
        public int GridHeight => grid_height;
        public int MoveCount => move_count;
        /// <summary>
        /// r: Red, g: Green, b: Blue, y: Yellow, rand: One of random colors(r, g, b, y),
        /// t: TNT, rov: Vertical Rocket, roh: Horizontal Rocket, bo: Box, s: Stone, v: Vase
        /// </summary>
        public string[] GridData => grid;
    }
}