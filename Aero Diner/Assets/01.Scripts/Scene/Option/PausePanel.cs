using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PausePanel : MonoBehaviour
{
    [SerializeField] private List<GameObject> pauseButtons;

    public void PlaySequentialIntro()
    {
        foreach (var btn in pauseButtons)
        {
            btn.SetActive(false); // 시작 시 모두 꺼두기
        }

        StartCoroutine(SequentialActivate());
    }

    private IEnumerator SequentialActivate()
    {
        foreach (var btn in pauseButtons)
        {
            yield return new WaitForSecondsRealtime(0.1f); // TimeScale=0에도 동작
            btn.SetActive(true);
            EventBus.PlaySFX(SFXType.OpenPause);
        }
    }
}
