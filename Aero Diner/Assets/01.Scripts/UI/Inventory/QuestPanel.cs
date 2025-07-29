
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using UnityEngine.UI; 

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
        // 초기화 시 호출되는 경우
        if (quest == null)
        {
            questNameText.text = "";
            descriptionText.text = "";
            rewardText.text = "";
            objectiveText.text = "";
            return;
        }

        // 실제 데이터 바인딩
        questNameText.text = quest.questName;
        descriptionText.text = quest.description;
        rewardText.text = $"보상: {quest.rewardDescription} ({quest.rewardMoney}G)";

        foreach (var obj in quest.objectives)
        {
            int currentAmount;
            
            if (obj.objectiveType == QuestObjectiveType.EarnMoney)
            {
                // 돈 퀘스트만 GameManager에서 현재까지 번 총액 가져오기
                currentAmount = GameManager.Instance.TotalEarnings; 
            }
            else
            {
                currentAmount = QuestManager.Instance.GetQuestObjectiveProgress(quest.id, obj.targetId);
            }
            
            currentAmount = Mathf.Min(currentAmount, obj.requiredAmount);
            
            objectiveText.text = $"• {obj.description} ({currentAmount} / {obj.requiredAmount})";
        }
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

