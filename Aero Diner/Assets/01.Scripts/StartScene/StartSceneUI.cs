using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSceneUI : MonoBehaviour
{
    public void OnClickStartGame()
    {
        FadeManager.Instance.FadeOutAndLoadSceneWithLoading("MainScene");
    }
}
