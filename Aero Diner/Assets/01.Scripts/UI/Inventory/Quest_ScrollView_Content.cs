using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Quest_ScrollView_Content : BaseScrollViewItem
{
    [SerializeField] private TMP_Text titleText;
    private QuestData questData;

    public Action OnClicked;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => OnClicked?.Invoke());
    }

    public void SetData(QuestData data)
    {
        questData = data;
        titleText.text = data.questName;
    }
}
