using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;

public class PressAnyKeyBlinker : MonoBehaviour
{
    [SerializeField] private Image pressAnyKeyImage; 
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float blinkInterval = 0.5f;
    [SerializeField] private float fadeOutDuration = 1f;

    [Header("Menu Panels")]
    [SerializeField] private GameObject menuPanel3;
    [SerializeField] private GameObject menuPanel4;

    private bool inputDetected = false;

    private void Start()
    {
        StartCoroutine(BlinkImage());
    }

    private void Update()
    {
        if (inputDetected) return;

        if (Input.anyKeyDown)
        {
            inputDetected = true;
            StopAllCoroutines();
            FadeOutAndShowMenu();
        }
    }

    private IEnumerator BlinkImage()
    {
        while (true)
        {
            pressAnyKeyImage.enabled = !pressAnyKeyImage.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private void FadeOutAndShowMenu()
    {
        canvasGroup.DOFade(0f, fadeOutDuration)
            .OnComplete(() =>
            {
                Debug.Log("Fade 완료. 메뉴 표시 시작");

                if (HasSavedGame())
                {
                    menuPanel4.SetActive(true);
                }
                else
                {
                    menuPanel3.SetActive(true);
                }

                pressAnyKeyImage.enabled = false;
            });
    }

    private bool HasSavedGame()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "userdata.json");
        return System.IO.File.Exists(path);
    }
}