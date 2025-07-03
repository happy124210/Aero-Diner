using UnityEngine;

public class FoodDisplay : MonoBehaviour, IInteractable
{
    public FoodData foodData;
    public IPlaceableStation originPlace;


    public void Interact(PlayerInventory playerInventory, InteractionType interactionType)
    {
        if (!playerInventory) return;

        if (interactionType == InteractionType.Pickup)
        {
            playerInventory.TryPickup(this);
        }
        else
        {
            Debug.Log("FoodDisplay는 상호작용할 기능이 없습니다.");
        }
    }

    public void OnHoverEnter()
    {

    }

    public void OnHoverExit()
    {

    }
}
