using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SavePopupFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup popupGroup; // 페이드 제어용
    [SerializeField] private float showDuration = 2f;
    [SerializeField] private float fadeDuration = 1f;

    private Coroutine currentRoutine;

    public void ShowPopup(string message)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowAndFade(message));
    }

    private IEnumerator ShowAndFade(string message)
    {
        popupGroup.gameObject.SetActive(true);

        TMP_Text text = popupGroup.GetComponentInChildren<TMP_Text>();
        if (text != null) text.text = message;

        popupGroup.alpha = 1f;

        yield return new WaitForSeconds(showDuration);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            popupGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        popupGroup.alpha = 0f;
        popupGroup.gameObject.SetActive(false);
        currentRoutine = null;
    }
}
