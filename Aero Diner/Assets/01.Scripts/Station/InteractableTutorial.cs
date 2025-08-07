using UnityEngine;

/// <summary>
/// 튜토리얼용 상호작용 관리 스크립트.
/// 오브젝트가 상호작용 가능한 상태일 경우:
/// - 원래 색상으로 복원
/// - 레이어를 "Interactable"(6번)로 설정
/// 비활성 상태일 경우:
/// - 색상을 회색으로 변경
/// - 레이어를 "Default"(0번)로 설정
/// </summary>
public class InteractableTutorial : MonoBehaviour
{
    [SerializeField] private bool isIInteractable = false;                 // 오브젝트가 상호작용 가능한지 여부 (true: 가능, false: 불가능)
    [SerializeField] private SpriteRenderer spriteRenderer;                // 오브젝트의 SpriteRenderer 컴포넌트 참조
    [SerializeField] private Color defaultColor = Color.white;             // 원래의 색상 (상호작용 가능 시 적용)
    [SerializeField] private Color disabledColor = Color.gray;             // 비활성 상태일 때 적용할 색상
    [SerializeField] private FoodSlotIconDisplay foodSlotIconDisplay;      // FoodSlotIconDisplay 컴포넌트 참조

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        UpdateInteractableState();
    }

    /// <summary>
    /// 외부에서 상호작용 상태를 설정할 때 사용
    /// </summary>
    /// <param name="state">true면 활성, false면 비활성</param>
    public void SetInteractable(bool state)
    {
        isIInteractable = state;
        UpdateInteractableState();
    }

    /// <summary>
    /// 현재 isIInteractable 상태에 따라 색상과 레이어를 변경
    /// </summary>
    private void UpdateInteractableState()
    {
        if (spriteRenderer == null) return;

        if (isIInteractable)
        {
            // 상호작용 가능: 색상을 원래대로, 레이어를 Interactable로 설정
            spriteRenderer.color = defaultColor;
            gameObject.layer = LayerMask.NameToLayer("Interactable");
            if (foodSlotIconDisplay != null)
            {
                foodSlotIconDisplay.ResetAll(); 
            }
        }
        else
        {
            // 상호작용 불가능: 색상을 회색으로, 레이어를 Default로 설정
            spriteRenderer.color = disabledColor;
            gameObject.layer = LayerMask.NameToLayer("Default");
            if (foodSlotIconDisplay != null)
            {
                foodSlotIconDisplay.CloseSlot();
            }
        }
    }
}