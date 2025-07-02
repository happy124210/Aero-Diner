using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRoot : Singleton<UIRoot>
{
    public KeyRebindManager keyRebindManager;
    public VolumeHandler volumeHandler;
    public VideoSettingPanel videoSettingPanel;
    public TabButtonController tabButtonController;

    [SerializeField]
    public GameObject pausePanel;
    public GameObject optionPanel;
    public GameObject volumePanel;
    public GameObject videoPanel;
    public GameObject keysettingPanel;


    public UITracker uiTracker;
    public UIExitPopup uiExitPopup;
}
