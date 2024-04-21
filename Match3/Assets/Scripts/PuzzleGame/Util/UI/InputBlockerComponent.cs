using System;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Util.UI
{
    public class InputBlockerComponent : MonoBehaviour
    {
        [SerializeField] private Image raycastImage;
        
        public static InputBlockerComponent Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        public void ChangeInteractable(bool state)
        {
            raycastImage.raycastTarget = state;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}