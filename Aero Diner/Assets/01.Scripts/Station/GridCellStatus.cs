using UnityEngine;

/// <summary>
/// GridCell의 상태를 판단하고 Sprite 및 Outline을 제어
/// </summary>
public class GridCellStatus : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    private OutlineShaderController outline;
    private Material placeableMaterial;
    private Material notPlaceableMaterial;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        outline = GetComponent<OutlineShaderController>();
    }

    /// <summary>
    /// 현재 자식 유무에 따라 배치 가능 여부를 판단하고 상태 업데이트
    /// </summary>
    public void UpdateStatus()
    {
        bool isPlaceable = transform.childCount == 0;

        if (sr != null)
            sr.sharedMaterial = isPlaceable ? placeableMaterial : notPlaceableMaterial;

        if (outline != null)
        {
            if (isPlaceable)
                outline.DisableOutline();
            else
                outline.EnableOutline();
        }
    }

#if UNITY_EDITOR
    // 에디터에서 강제로 Outline 켜기/끄기 토글 테스트용 함수
    [ContextMenu("Force Enable Outline")]
    public void ForceEnableOutline()
    {
        if (outline != null)
            outline.EnableOutline();
    }

    [ContextMenu("Force Disable Outline")]
    public void ForceDisableOutline()
    {
        if (outline != null)
            outline.DisableOutline();
    }
#endif

    public void SetMaterials(Material placeable, Material notPlaceable)
    {
        placeableMaterial = placeable;
        notPlaceableMaterial = notPlaceable;
    }

    public void Interact(PlayerInventory inventory, InteractionType interactionType) { }
    public void OnHoverEnter()
    {
        outline?.EnableOutline();
    }
    public void OnHoverExit()
    {
        outline?.DisableOutline();
    }
}