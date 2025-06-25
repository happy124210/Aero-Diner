using UnityEngine;

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
        switch (eventType)
        {
            case UIEventType.OpenPause:
                if (pausePanel != null) pausePanel.SetActive(true);
                break;

            case UIEventType.ClosePause:
                if (pausePanel != null) pausePanel.SetActive(false);
                break;

            case UIEventType.OpenOption:
                pausePanel.SetActive(false);
                optionPanel.SetActive(true);
                soundPanel.SetActive(true); // 기본 탭: 사운드
                videoPanel.SetActive(false);
                controlPanel.SetActive(false);
                break;
            case UIEventType.CloseOption:
                pausePanel.SetActive(true);
                optionPanel.SetActive(false);
                soundPanel.SetActive(false);
                videoPanel.SetActive(false);
                controlPanel.SetActive(false);
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