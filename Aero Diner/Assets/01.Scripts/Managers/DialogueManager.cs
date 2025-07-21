using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : Singleton<DialogueManager>
{
    [Header("Debug Info")]
    [SerializeField] public bool showDebugInfo;
    
    private Queue<DialogueLine> linesQueue;
    private DialogueData currentDialogue;
    
    private Dictionary<string, DialogueData> dialogueDatabase;
    
    protected override void Awake()
    {
        base.Awake();
        linesQueue = new Queue<DialogueLine>();
        
        LoadDialogueDatabase();
    }

    #region 데이터베이스 관리

    private void LoadDialogueDatabase()
    {
        dialogueDatabase = new Dictionary<string, DialogueData>();
        
        DialogueData[] allDialogues = Resources.LoadAll<DialogueData>("Datas/Dialogue");
        
        foreach (DialogueData dialogue in allDialogues)
        {
            if (!dialogueDatabase.ContainsKey(dialogue.id))
            {
                dialogueDatabase.Add(dialogue.id, dialogue);
            }
            else
            {
                Debug.LogWarning($"중복된 Dialogue ID 있음: {dialogue.id}");
            }
        }
        
        if (showDebugInfo) Debug.Log($"총 {dialogueDatabase.Count}개 Dialogue Database 로드 완료");
    }
    
    public DialogueData FindDialogueDataById(string id)
    {
        if (dialogueDatabase.TryGetValue(id, out DialogueData data))
        {
            return data;
        }
        
        Debug.LogWarning($"id: {id} 없음");
        return null;
    }

    #endregion

    /// <summary>
    /// 새로운 대화 시작
    /// </summary>
    /// <param name="data"> 시작할 DialogueData SO </param>
    public void StartDialogue(DialogueData data)
    {
        if (data == null)
        {
            if (showDebugInfo) Debug.LogError("시작할 DialogueData 없음");
            // 대화 시작 실패 시, 이전 페이즈 복귀
            GameManager.Instance.ContinueGame();
            return;
        }
        
        GameManager.Instance.ChangePhase(GamePhase.Dialogue);

        linesQueue.Clear();
        foreach (var line in data.lines)
        {
            linesQueue.Enqueue(line);
        }
        
        currentDialogue = data;
        RequestNextLine();
    }
    
    /// <summary>
    /// 다음 대사 요청
    /// 다음라인 있으면 이벤트로 string 넘겨줌, 없으면 선택지 확인
    /// </summary>
    public void RequestNextLine()
    {
        if (linesQueue.Count > 0)
        {
            DialogueLine lineToShow = linesQueue.Dequeue();
            // TODO: 이벤트 발생 여부 논의
            //OnShowLine?.Invoke(lineToShow);
        }
        else
        {
            ShowChoicesOrEnd();
        }
    }
    
    /// <summary>
    /// 선택지 확인
    /// 보여줄 선택지 있으면 event로 choices 넘겨주고, 없으면 종료
    /// </summary>
    private void ShowChoicesOrEnd()
    {
        if (currentDialogue.choices != null && currentDialogue.choices.Count > 0)
        {
            // TODO: 이벤트 발생 여부 논의
            // OnShowChoices?.Invoke(currentDialogue.choices);
        }
        else
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// 플레이어가 선택지를 골랐을 때
    /// 선택지에 연결된 다음 대화가 있다면 연결, 없으면 종료
    /// </summary>
    /// <param name="choiceIndex"> 선택한 선택지의 인덱스 </param>
    public void SelectChoice(int choiceIndex)
    {
        // 유효하지 않은 선택이면 대화 종료
        if (choiceIndex < 0 || choiceIndex >= currentDialogue.choices.Count)
        {
            EndDialogue();
            return;
        }
        
        DialogueChoice selectedChoice = currentDialogue.choices[choiceIndex];

        // 이 선택지에 연결된 다음 대화가 있다면 연결
        if (!string.IsNullOrEmpty(selectedChoice.nextDialogueId))
        {
            DialogueData nextDialogue = FindDialogueDataById(selectedChoice.nextDialogueId);
            StartDialogue(nextDialogue);
        }
        // 없다면 종료
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        // TODO: EventBus.Raise(UIEventType.HideDialogue);
        // TODO: 후속 이벤트 전달 로직
        GameManager.Instance.ContinueGame();
    }
}