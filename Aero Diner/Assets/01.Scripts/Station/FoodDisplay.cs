using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FoodDisplay : MonoBehaviour, IInteractable
{
    public ScriptableObject rawData;
    public IngredientStation originIngredient;
    public IPlaceableStation originPlace;

    // IIngredientData로 접근할 수 있도록 추가
    public CookingSOGroup.IIngredientData data => rawData as CookingSOGroup.IIngredientData;


    public void Interact(PlayerInventory playerInventory, InteractionType interactionType)
    {
        if (playerInventory == null) return;

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
