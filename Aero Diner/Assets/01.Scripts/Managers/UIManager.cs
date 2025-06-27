using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("패널들")]
    public GameObject pausePanel;
    public GameObject optionPanel;
    public GameObject soundPanel;
    public GameObject videoPanel;
    public GameObject controlPanel;

    [Header("설정 컴포넌트들")]
    public VolumeHandler volumeHandler;
    public VideoSettingPanel videoSettingPanel;
    public KeyRebindManager keyRebindManager;
    public UIExitPopup uiExitPopup;
    public SavePopupFader savePopupFader;
    public UIInputHandler uiInputHandler;
    public UITracker uiTracker;
    public OptionBtn optionBtn;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
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
                pausePanel?.SetActive(false);
                optionPanel?.SetActive(true);
                soundPanel?.SetActive(true);
                videoPanel?.SetActive(false);
                controlPanel?.SetActive(false);
                break;
            case UIEventType.CloseOption:
                if (!isStartScene) pausePanel?.SetActive(true);
                optionPanel?.SetActive(false);
                soundPanel?.SetActive(false);
                videoPanel?.SetActive(false);
                controlPanel?.SetActive(false);
                break;
            case UIEventType.ShowSoundTab:
                soundPanel?.SetActive(true);
                videoPanel?.SetActive(false);
                controlPanel?.SetActive(false);
                break;
            case UIEventType.ShowVideoTab:
                soundPanel?.SetActive(false);
                videoPanel?.SetActive(true);
                controlPanel?.SetActive(false);
                break;
            case UIEventType.ShowControlTab:
                soundPanel?.SetActive(false);
                videoPanel?.SetActive(false);
                controlPanel?.SetActive(true);
                break;
        }
    }
}