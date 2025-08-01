using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class DialogueUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image leftPortraitImage;
    [SerializeField] private Image rightPortraitImage;
    [SerializeField] private GameObject nextButton;          
    [SerializeField] private CanvasGroup nextButtonGroup;

    private string leftSpeakerId = null;
    private string rightSpeakerId = null;
    private bool initialized = false;

    private Tween typingTween;
    private Tween blinkingTween;
    private HashSet<string> appearedSpeakers = new(); //등장 화자 추적

    private void OnEnable()
    {
        EventBus.OnUIEvent += HandleUIEvent;
        appearedSpeakers.Clear(); // 대화 시작 시 초기화
        nextButton.SetActive(false);

        leftPortraitImage.sprite = null;
        rightPortraitImage.sprite = null;
        SetAlpha(leftPortraitImage, 0f);
        SetAlpha(rightPortraitImage, 0f);
    }

    private void OnDisable()
    {
        EventBus.OnUIEvent -= HandleUIEvent;
    }
    private void Update()
    {
        if (!rootPanel.activeInHierarchy) return;

        // nextButton이 깜빡이는 중일 때만 허용
        if (nextButton.activeSelf && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            OnClickNext();
        }
    }
    private void HandleUIEvent(UIEventType type, object payload)
    {
        switch (type)
        {
            case UIEventType.ShowDialogueLine:
                if (payload is DialogueLine line)
                    DisplayLine(line);
                break;

            case UIEventType.HideDialoguePanel:
                rootPanel.SetActive(false);
                break;
        }
    }

    private void DisplayLine(DialogueLine line)
    {
        rootPanel.SetActive(true);

        // 화자 자동 설정
        if (!initialized)
        {
            leftSpeakerId = line.speakerId;
            initialized = true;
        }
        else if (rightSpeakerId == null && line.speakerId != leftSpeakerId)
        {
            rightSpeakerId = line.speakerId;
        }

        var speaker = DialogueManager.Instance.FindSpeakerById(line.speakerId);
        Sprite portrait = speaker?.GetPortraitByExpression(line.expression);
        string name = speaker?.speakerName ?? line.speakerId;

        speakerNameText.text = name;

        Image activePortrait = null;
        Image inactivePortrait = null;

        if (line.speakerId == leftSpeakerId)
        {
            SetAlpha(leftPortraitImage, 0f);
            leftPortraitImage.sprite = portrait;
            activePortrait = leftPortraitImage;
            inactivePortrait = rightPortraitImage;
        }
        else if (line.speakerId == rightSpeakerId)
        {
            SetAlpha(rightPortraitImage, 0f);
            rightPortraitImage.sprite = portrait;
            activePortrait = rightPortraitImage;
            inactivePortrait = leftPortraitImage;
        }

        if (activePortrait != null)
        {
            SetAlpha(activePortrait, 1f);

            if (inactivePortrait != null)
            {
                string inactiveId = (line.speakerId == leftSpeakerId) ? rightSpeakerId : leftSpeakerId;
                bool inactiveHasAppeared = !string.IsNullOrEmpty(inactiveId) && appearedSpeakers.Contains(inactiveId);

                if (inactiveHasAppeared)
                {
                    SetAlpha(inactivePortrait, 0.3f);
                }
                else
                {
                    inactivePortrait.sprite = null;
                    SetAlpha(inactivePortrait, 0f);
                }
            }
        }

        // 처음 등장한 화자면 강조 애니메이션
        if (!appearedSpeakers.Contains(line.speakerId) && activePortrait != null)
        {
            appearedSpeakers.Add(line.speakerId);

            activePortrait.rectTransform.localScale = Vector3.one;
            activePortrait.rectTransform
                .DOScale(1.1f, 0.15f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
        }

        // 텍스트 타이핑 애니메이션
        typingTween?.Kill();
        dialogueText.text = "";

        typingTween = DOTween.To(() => "", x => dialogueText.text = x, line.text, 0.03f * line.text.Length)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                StartBlinkingNextButton();
            });
    }

    private void SetAlpha(Image image, float alpha)
    {
        if (image == null) return;
        var c = image.color;
        c.a = alpha;
        image.color = c;
    }
    private void StartBlinkingNextButton()
    {
        nextButton.SetActive(true);
        nextButtonGroup.alpha = 1f;

        blinkingTween?.Kill();
        blinkingTween = nextButtonGroup.DOFade(0f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopBlinkingNextButton()
    {
        blinkingTween?.Kill();
        nextButton.SetActive(false);
    }
    public void OnClickNext()
    {
        if (typingTween != null && typingTween.IsActive() && typingTween.IsPlaying())
        {
            typingTween.Complete();
            return;
        }
        StopBlinkingNextButton();
        DialogueManager.Instance.RequestNextLine();
    }
    public void OnClickSkip()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        typingTween?.Kill();
        blinkingTween?.Kill();
        nextButton.SetActive(false);

        DialogueManager.Instance.SkipDialogue();
        rootPanel.SetActive(false);
    }
}
