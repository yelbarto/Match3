using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Gameplay.Views
{
    public class DebugView : MonoBehaviour
    {
        [SerializeField] private Button debugButton;
        [SerializeField] private GameObject debugPanel;
        [SerializeField] private Button changeLevelButton;
        [SerializeField] private Button restartLevelButton;
        [SerializeField] private Button completeLevelButton;
        [SerializeField] private Button failLevelButton;
        [SerializeField] private TMP_InputField levelInputField;
        
        [NonSerialized] public Action<int> ChangeLevelAction;
        [NonSerialized] public Action RestartLevelAction;
        [NonSerialized] public Action CompleteLevelAction;
        [NonSerialized] public Action FailLevelAction;

        private void Awake()
        {
            debugPanel.SetActive(false);
            debugButton.onClick.AddListener(() => debugPanel.SetActive(!debugPanel.activeSelf));
            changeLevelButton.onClick.AddListener(ChangeLevel);
            restartLevelButton.onClick.AddListener(() =>
            {
                RestartLevelAction?.Invoke();
                debugPanel.SetActive(false);
            });
            completeLevelButton.onClick.AddListener(() =>
            {
                CompleteLevelAction?.Invoke();
                debugPanel.SetActive(false);
            });
            failLevelButton.onClick.AddListener(() =>
            {
                FailLevelAction?.Invoke();
                debugPanel.SetActive(false);
            });
        }

        private void ChangeLevel()
        {
            if (int.TryParse(levelInputField.text, out var level))
            {
                ChangeLevelAction?.Invoke(level);
                debugPanel.SetActive(false);
                return;
            }
            Debug.LogError("Could not parse level input field! " + levelInputField.text);
        }

        private void OnDestroy()
        {
            debugButton.onClick.RemoveAllListeners();
            changeLevelButton.onClick.RemoveAllListeners();
            restartLevelButton.onClick.RemoveAllListeners();
            completeLevelButton.onClick.RemoveAllListeners();
            failLevelButton.onClick.RemoveAllListeners();
        }
    }
}