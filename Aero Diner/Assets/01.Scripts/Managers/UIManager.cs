using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    [Header("패널들")]
    public GameObject pausePanel;
    public GameObject optionPanel;
    public GameObject soundPanel;
    public GameObject videoPanel;
    public GameObject controlPanel;
    public GameObject menuPanel3;
    public GameObject menuPanel4;

    [Header("설정 컴포넌트들")]
    public VolumeHandler volumeHandler;
    public VideoSettingPanel videoSettingPanel;
    public KeyRebindManager keyRebindManager;
    public UIExitPopup uiExitPopup;
    public SavePopupFader savePopupFader;
    public UIInputHandler uiInputHandler;
    public UITracker uiTracker;
    public OptionBtn optionBtn;

    [Header("기타 UI")]
    public GameObject roundTimerPanel;


    [SerializeField] private EarningsDisplay earningsDisplay;

    private void OnEnable()
    {
        EventBus.OnUIEvent += HandleUIEvent;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        EventBus.OnUIEvent -= HandleUIEvent;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LoadingScene")
        {
            DisableAllPanels();
        }
    }
    private void DisableAllPanels()
    {
        pausePanel?.SetActive(false);
        optionPanel?.SetActive(false);
        soundPanel?.SetActive(false);
        videoPanel?.SetActive(false);
        controlPanel?.SetActive(false);
        menuPanel3?.SetActive(false);
        menuPanel4?.SetActive(false);
        uiExitPopup?.gameObject.SetActive(false);
        savePopupFader?.gameObject.SetActive(false);
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
            case UIEventType.ShowStartMenuWithSave:
                menuPanel4.SetActive(true);
                break;

            case UIEventType.ShowStartMenuNoSave:
                menuPanel3.SetActive(true);
                break;
            // UIManager.cs 내부 HandleUIEvent(UIEventType eventType, object payload)
            case UIEventType.LoadMainScene:
                FadeManager.Instance.FadeOutAndLoadSceneWithLoading("MainScene");
                break;

            case UIEventType.QuitGame:
                Debug.Log("게임 종료 요청됨");
                break;

            case UIEventType.ShowRoundTimer:
                roundTimerPanel?.SetActive(true);
                break;

            case UIEventType.HideRoundTimer:
                roundTimerPanel?.SetActive(false);
                break;

            case UIEventType.UpdateEarnings:
                earningsDisplay?.AnimateEarnings((float)payload);
                break;

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
                break;
        }
    }
}