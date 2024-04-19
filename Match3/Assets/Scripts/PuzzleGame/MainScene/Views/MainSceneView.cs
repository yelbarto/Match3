using System;
using PuzzleGame.MainScene.Components;
using Sirenix.OdinInspector;
using UnityEngine;

public class MainSceneView : MonoBehaviour
{
    private const string REF = "References";
    
    [SerializeField, FoldoutGroup(REF)] private LevelButtonComponent levelButtonComponent;
    [SerializeField, FoldoutGroup(REF)] private GameObject mainSceneUi;

    public event Action OpenLevelDelegate;
    
    private void Start()
    {
        levelButtonComponent.OnPlayButtonClicked += PlayButtonClicked;
    }

    public void SetUp(int currentLevel)
    {
        mainSceneUi.SetActive(true);
        levelButtonComponent.SetUp(currentLevel);
    }

    public void Close()
    {
        mainSceneUi.SetActive(false);
    }
    
    private void PlayButtonClicked()
    {
        OpenLevelDelegate?.Invoke();
    }

    private void OnDestroy()
    {
        levelButtonComponent.OnPlayButtonClicked -= PlayButtonClicked;
    }
}
