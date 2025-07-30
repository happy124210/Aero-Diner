using System.Text;
using TMPro;
using UnityEngine;

public class QuestPanel : MonoBehaviour
{
    [Header("탭 및 패널")]
    [SerializeField] private TabController tabController;
    [SerializeField] private GameObject noQuestPanel;

    [Header("ScrollView Content 부모")]
    [SerializeField] private Transform doingContent;
    [SerializeField] private Transform completeContent;

    [Header("리스트 아이템 프리팹")]
    [SerializeField] private GameObject questListItemPrefab;

    [Header("퀘스트 상세정보")]
    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text rewardText;

    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo;

    private void OnEnable()
    {
        if (tabController == null)
            tabController = GetComponentInChildren<TabController>();

        tabController.RequestSelectTab(0, true); // 첫 번째 탭 선택
        tabController.ApplyTabSelectionVisuals();
        RefreshQuestLists();
    }
    
    #region 버튼메서드
    
    public void OnClickDoningBtn()
    {
        if (showDebugInfo)
            Debug.Log("버튼 클릭 됨");
        EventBus.PlaySFX(SFXType.ButtonClick);
        tabController.RequestSelectTab(0);
        tabController.ApplyTabSelectionVisuals();
        RefreshQuestLists();
    }
    
    public void OnClickCompleteBtn()
    {
        if (showDebugInfo)
            Debug.Log("버튼 클릭 됨");
        EventBus.PlaySFX(SFXType.ButtonClick);
        tabController.RequestSelectTab(1);
        tabController.ApplyTabSelectionVisuals();
        RefreshQuestLists();
    }
    
    #endregion
    
    private void RefreshQuestLists()
    {
        ClearChildren(doingContent);
        ClearChildren(completeContent);

        var doingQuests = QuestManager.Instance.GetInProgressQuests();
        var completeQuests = QuestManager.Instance.GetCompletedQuests();

        foreach (var quest in doingQuests)
        {
            CreateQuestListItem(quest, doingContent);
        }

        foreach (var quest in completeQuests)
        {
            CreateQuestListItem(quest, completeContent);
        }

        // 공용 퀘스트 없음 패널 처리
        bool hasNoQuests = doingQuests.Count == 0 && completeQuests.Count == 0;

        if (noQuestPanel != null)
            noQuestPanel.SetActive(hasNoQuests);

        // 상세 패널 초기화
        ShowQuestDetail(null);
    }
    
    private void CreateQuestListItem(QuestData questData, Transform parent)
    {
        GameObject go = Instantiate(questListItemPrefab, parent);
        var item = go.GetComponent<Quest_ScrollView_Content>();
        item.SetData(questData);
        item.OnClicked = () => ShowQuestDetail(questData);
    }
    
    private void ShowQuestDetail(QuestData quest)
    {
        if (quest == null)
        {
            questNameText.text = "";
            descriptionText.text = "";
            rewardText.text = "";
            objectiveText.text = "";
            return;
        }

        // --- 퀘스트 기본 정보 표시 ---
        questNameText.text = quest.questName;
        descriptionText.text = quest.description;
    
        // 보상 정보가 있을 경우에만 표시
        if (!string.IsNullOrEmpty(quest.rewardDescription) || quest.rewardMoney > 0)
        {
            rewardText.text = $"보상: {quest.rewardDescription}";
        }
        else
        {
            rewardText.text = "보상: 없음";
        }

        // 모든 퀘스트 목표(Objective)를 순회하며 텍스트 생성
        StringBuilder objectiveBuilder = new StringBuilder();

        foreach (var obj in quest.objectives)
        {
            int currentAmount = 0;
            int requiredAmount = 1;
            
            if (obj.requiredIds != null && obj.requiredIds.Length > 0 && int.TryParse(obj.requiredIds[0], out int req))
            {
                requiredAmount = req;
            }

            // 목표 타입에 따라 현재 진행도를 가져옵니다.
            currentAmount = obj.objectiveType == QuestObjectiveType.EarnMoney 
                ? GameManager.Instance.TotalEarnings 
                : QuestManager.Instance.GetQuestObjectiveProgress(quest.id, obj.targetId);
            
            currentAmount = Mathf.Min(currentAmount, requiredAmount);
            objectiveBuilder.AppendLine($"• {obj.description} ({currentAmount} / {requiredAmount})");
        }
        
        objectiveText.text = objectiveBuilder.ToString();
    }
    
    private void ClearChildren(Transform container)
    {
        if (container == null) return;

        foreach (Transform child in container)
        {
            if (child == null) continue;

            UnityEngine.UI.Button btn = child.GetComponentInChildren<UnityEngine.UI.Button>();
            if (btn != null)
                btn.onClick.RemoveAllListeners();

            Destroy(child.gameObject);
        }
    }
}

