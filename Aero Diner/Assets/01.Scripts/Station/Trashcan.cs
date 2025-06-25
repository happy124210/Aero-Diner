using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trashcan : MonoBehaviour
{

    public bool PlaceIngredient(FoodData data)
    {
        return true; // 어떤 재료든 내려놓기 허용
        //플레이어가 호출 할 때 참고용 코드
        //bool canPlace = trashcan.PlaceIngredient(playerHoldingData);
        //if (canPlace)
        //{
        //    playerInventory.DropItem(); // 인벤토리에서 제거
        //}

    }
}
