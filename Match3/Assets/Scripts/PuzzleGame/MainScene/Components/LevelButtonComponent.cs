using System;
using PuzzleGame.Util.UI;
using TMPro;
using UnityEngine;

namespace PuzzleGame.MainScene.Components
{
    public class LevelButtonComponent : MonoBehaviour
    {
        [SerializeField] private PersonalizedButton button;
        [SerializeField] private TMP_Text buttonText;

        public event Action OnPlayButtonClicked;

        private void Awake()
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            button.interactable = false;
            OnPlayButtonClicked?.Invoke();
        }

        public void SetUp(int level)
        {
            button.interactable = level != -1;
            buttonText.text = level == -1 ? "Finished" : $"Level {level}";
        }

        private void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
        }
    }
}