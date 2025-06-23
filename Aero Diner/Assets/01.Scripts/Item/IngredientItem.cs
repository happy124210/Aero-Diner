using UnityEngine;

public class IngredientItem : MonoBehaviour, IInteractable, IItem
{
    public ScriptableObject ingredientData; // 식별용 데이터 (선택)

    public Sprite icon;           // UI에 표시할 스프라이트
    public string itemName = "재료 이름";

    public void Interact(PlayerInventory inventory)
    {
        inventory.TryPickup(this);
    }

    public string GetItemName()
    {
        return itemName;
    }

    public Sprite GetSprite()
    {
        return icon;
    }

    public void OnHoverEnter()
    {
        // 선택 효과 등
    }

    public void OnHoverExit()
    {
        // 선택 해제 효과 등
    }
    public void Use()
    {

    }
}
