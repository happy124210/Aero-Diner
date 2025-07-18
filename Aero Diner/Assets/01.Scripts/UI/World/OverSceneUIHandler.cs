using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OverSceneUIHandler : IUIEventHandler
{
    private readonly List<GameObject> sceneUIs;
    public OverSceneUIHandler(List<GameObject> sceneUIs)
    {
        this.sceneUIs = sceneUIs;
    }
    public bool Handle(UIEventType type, object payload)
    {
        var pausePanel = UIRoot.Instance.pausePanel?.GetComponent<PausePanelEffecter>();
        switch (type)
        {
            case UIEventType.OpenPause:
                UIRoot.Instance.pausePanel?.SetActive(true);
                GameManager.Instance.PauseGame();
                pausePanel?.PlaySequentialIntro(); // 칼 등장 애니메이션
                break;

            case UIEventType.ClosePause:
                GameManager.Instance.ContinueGame();
                pausePanel?.HideWithPushEffect();  // 위로 밀려서 사라지는 연출
                break;
            case UIEventType.OpenOption:
                UIRoot.Instance.pausePanel.SetActive(false);
                UIRoot.Instance.optionPanel.SetActive(true);
                UIRoot.Instance.optionPanel.GetComponent<OptionPanelEffecter>()?.PlayFadeIn();
                UIRoot.Instance.volumePanel.gameObject.SetActive(true);
                break;
            case UIEventType.CloseOption:
                UIRoot.Instance.optionPanel.GetComponent<OptionPanelEffecter>()?.PlayFadeOut();
                if (SceneManager.GetActiveScene().name != "StartScene")
                    UIRoot.Instance.pausePanel.SetActive(true);
                break;
            case UIEventType.ShowSoundTab:
                UIRoot.Instance.volumePanel.gameObject.SetActive(true);
                UIRoot.Instance.videoPanel.gameObject.SetActive(false);
                UIRoot.Instance.keysettingPanel.gameObject.SetActive(false);
                break;
            case UIEventType.ShowVideoTab:
                UIRoot.Instance.volumePanel.gameObject.SetActive(false);
                UIRoot.Instance.videoPanel.gameObject.SetActive(true);
                UIRoot.Instance.keysettingPanel.gameObject.SetActive(false);
                break;
            case UIEventType.ShowControlTab:
                UIRoot.Instance.volumePanel.gameObject.SetActive(false);
                UIRoot.Instance.videoPanel.gameObject.SetActive(false);
                UIRoot.Instance.keysettingPanel.gameObject.SetActive(true);
                break;
            case UIEventType.LoadMainScene:
                EventBus.RaiseFadeEvent(FadeEventType.FadeOutAndLoadScene, new FadeEventPayload(alpha: 1f, duration: 1f, scene: "MainScene"));
                break;
            case UIEventType.LoadDayScene:
                EventBus.RaiseFadeEvent(FadeEventType.FadeOutAndLoadScene, new FadeEventPayload(1f, 1f, scene: "DayScene"));
                break;
            case UIEventType.QuitGame:
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
            case UIEventType.ShowInventory:
                foreach (var ui in sceneUIs)
                {
                    var inventory = ui?.GetComponentInChildren<Inventory>(true);
                    var tab = inventory?.GetComponentInChildren<TabButtonController>(true);

                    inventory?.gameObject.SetActive(true);
                    ui?.GetComponentInChildren<IngredientPanel>(true)?.gameObject.SetActive(true);
                    ui?.GetComponentInChildren<RecipePanel>(true)?.gameObject.SetActive(false);
                    ui?.GetComponentInChildren<StationPanel>(true)?.gameObject.SetActive(false);
                    ui?.GetComponentInChildren<QuestPanel>(true)?.gameObject.SetActive(false);

                    tab?.RequestSelectTab(0);
                    tab?.ApplyTabSelectionVisuals();
                }
                return true;
            case UIEventType.ShowRecipeBook:
                foreach (var ui in sceneUIs)
                {
                    var inventory = ui?.GetComponentInChildren<Inventory>(true);
                    var tab = inventory?.GetComponentInChildren<TabButtonController>(true);

                    inventory?.gameObject.SetActive(true);
                    ui?.GetComponentInChildren<RecipePanel>(true)?.gameObject.SetActive(true);
                    ui?.GetComponentInChildren<StationPanel>(true)?.gameObject.SetActive(false);
                    ui?.GetComponentInChildren<IngredientPanel>(true)?.gameObject.SetActive(false);
                    ui?.GetComponentInChildren<QuestPanel>(true)?.gameObject.SetActive(false);

                    tab?.RequestSelectTab(2);
                    tab?.ApplyTabSelectionVisuals();
                }
                return true;
            case UIEventType.ShowStationPanel:
                foreach (var ui in sceneUIs)
                {
                    ui?.GetComponentInChildren<Inventory>(true)?.gameObject.SetActive(true);
                    ui?.GetComponentInChildren<StationPanel>(true)?.gameObject.SetActive(true);
                    ui?.GetComponentInChildren<IngredientPanel>(true)?.gameObject.SetActive(false);
                    ui?.GetComponentInChildren<RecipePanel>(true)?.gameObject.SetActive(false);
                    ui?.GetComponentInChildren<QuestPanel>(true)?.gameObject.SetActive(false);
                }
                return true;
            case UIEventType.ShowQuestPanel:
                foreach (var ui in sceneUIs)
                {
                    ui?.GetComponentInChildren<Inventory>(true)?.gameObject.SetActive(true);
                    ui?.GetComponentInChildren<StationPanel>(true)?.gameObject.SetActive(false);
                    ui?.GetComponentInChildren<IngredientPanel>(true)?.gameObject.SetActive(false);
                    ui?.GetComponentInChildren<RecipePanel>(true)?.gameObject.SetActive(false);
                    ui?.GetComponentInChildren<QuestPanel>(true)?.gameObject.SetActive(true);
                }
                return true;
            case UIEventType.HideInventory:
                foreach (var ui in sceneUIs)
                {
                    ui?.GetComponentInChildren<Inventory>(true)?.gameObject.SetActive(false);
                }
                return true;
        }
        return false;
    }
}
