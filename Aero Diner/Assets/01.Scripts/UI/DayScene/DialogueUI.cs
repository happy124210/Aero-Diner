using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogueUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image playerImage;
    [SerializeField] private Image npcImage;
    [SerializeField] private GameObject rootPanel;

    private void OnEnable()
    {
        EventBus.OnUIEvent += HandleUIEvent;
    }

    private void OnDisable()
    {
        EventBus.OnUIEvent -= HandleUIEvent;
    }

    private void HandleUIEvent(UIEventType type, object payload)
    {
        if (type == UIEventType.ShowDialogueLine)
        {
            if (payload is DialogueLine line)
                DisplayLine(line);
        }
        else if (type == UIEventType.HideDialoguePanel)
        {
            rootPanel.SetActive(false);
        }
    }

    private void DisplayLine(DialogueLine line)
    {
        rootPanel.SetActive(true);
        speakerNameText.text = line.speakerId;
        dialogueText.text = line.text;
        HighlightSpeaker(line.speakerId);
    }

    private void HighlightSpeaker(string speakerId)
    {
        if (speakerId == "A")
        {
            SetAlpha(playerImage, 1f);
            SetAlpha(npcImage, 0.3f);
        }
        else if (speakerId == "B")
        {
            SetAlpha(playerImage, 0.3f);
            SetAlpha(npcImage, 1f);
        }
        else
        {
            SetAlpha(playerImage, 1f);
            SetAlpha(npcImage, 1f);
        }
    }

    private void SetAlpha(Image image, float alpha)
    {
        var c = image.color;
        c.a = alpha;
        image.color = c;
    }

    public void OnClickNext()
    {
        DialogueManager.Instance.RequestNextLine();
    }
}
