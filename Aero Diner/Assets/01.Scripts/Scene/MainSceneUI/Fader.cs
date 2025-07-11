using UnityEngine;
using DG.Tweening;

public class Fader : MonoBehaviour
{
    [SerializeField] private CanvasGroup blackPanelGroup; // 알파 조절용
    [SerializeField] private float holdTime = 0.5f;
    [SerializeField] private float fadeDuration = 1f;

    private void Start()
    {
        PlayFadeIn();
    }

    public void PlayFadeIn()
    {
        blackPanelGroup.alpha = 1f;
        blackPanelGroup.blocksRaycasts = true;
        blackPanelGroup.interactable = true;

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(holdTime); // 3초 유지
        seq.Append(blackPanelGroup.DOFade(0f, fadeDuration)); // 1초 페이드
        seq.OnComplete(() =>
        {
            blackPanelGroup.blocksRaycasts = false;
            blackPanelGroup.interactable = false;
        });
    }
}
