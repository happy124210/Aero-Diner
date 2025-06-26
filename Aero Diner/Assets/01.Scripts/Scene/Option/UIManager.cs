using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private GameObject soundPanel;
    [SerializeField] private GameObject videoPanel;
    [SerializeField] private GameObject controlPanel;

    private void OnEnable()
    {
        EventBus.OnUIEvent += HandleUIEvent;
    }

    private void OnDisable()
    {
        EventBus.OnUIEvent -= HandleUIEvent;
    }

    private void HandleUIEvent(UIEventType eventType, object payload)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        bool isStartScene = currentScene == "StartScene";
        switch (eventType)
        {
            case UIEventType.OpenPause:
                if (pausePanel) pausePanel.SetActive(true);
                break;

            case UIEventType.ClosePause:
                if (pausePanel) pausePanel.SetActive(false);
                break;

            case UIEventType.OpenOption:
                pausePanel.SetActive(false);
                optionPanel.SetActive(true);
                soundPanel.SetActive(true); // 기본 탭: 사운드
                videoPanel.SetActive(false);
                controlPanel.SetActive(false);
                break;
            case UIEventType.CloseOption:
                if (!isStartScene && pausePanel) pausePanel.SetActive(true);
                if (optionPanel) optionPanel.SetActive(false);
                if (soundPanel) soundPanel.SetActive(false);
                if (videoPanel) videoPanel.SetActive(false);
                if (controlPanel) controlPanel.SetActive(false);
                break;

            case UIEventType.ShowSoundTab:
                soundPanel.SetActive(true);
                videoPanel.SetActive(false);
                controlPanel.SetActive(false);
                break;
            case UIEventType.ShowVideoTab:
                soundPanel.SetActive(false);
                videoPanel.SetActive(true);
                controlPanel.SetActive(false);
                break;
            case UIEventType.ShowControlTab:
                soundPanel.SetActive(false);
                videoPanel.SetActive(false);
                controlPanel.SetActive(true);
                break;
        }
    }
}