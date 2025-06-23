using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어가 상호작용하면 재료를 생성해주는 스테이션
/// </summary>
public class IngredientStation : ItemSlotStation
{
    [Header("재료 데이터 그룹")]
    public IngredientSOGroup ingredientGroup;

    [Header("생성할 재료 SO")]
    public ScriptableObject selectedIngredient;

    [Header("재료 생성 위치")]
    public Transform spawnPoint;

    /// <summary>
    /// 플레이어가 J 키를 눌렀을 때 실행되는 상호작용 메서드
    /// </summary>
    public override void Interact(PlayerInventory playerInventory)
    {
        // 필수 컴포넌트 검사
        if (ingredientGroup == null || selectedIngredient == null || spawnPoint == null)
        {
            Debug.LogError("필수 데이터가 누락되었습니다.");
            return;
        }

        // 선택된 재료 데이터를 캐스팅
        if (selectedIngredient is IngredientData ingredientData)
        {
            GameObject prefab = ingredientGroup.GetPrefabByData(ingredientData);
            if (prefab != null)
            {
                Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogError("선택된 ScriptableObject는 IngredientData 타입이 아닙니다.");
        }

        //// 새 GameObject를 생성하고 이름은 displayName으로 지정
        //GameObject ingredientObj = new GameObject(selectedIngredient.foodName);

        //// 생성 위치 지정
        //ingredientObj.transform.position = spawnPoint.position;

        //// SpriteRenderer 추가하여 foodIcon 스프라이트를 적용
        //SpriteRenderer spriteRenderer = ingredientObj.AddComponent<SpriteRenderer>();
        //if (selectedIngredient.foodIcon != null)
        //{
        //    spriteRenderer.sprite = selectedIngredient.foodIcon;
        //}
        //else
        //{
        //    // 스프라이트가 없는 경우 기본 회색으로 표시
        //    spriteRenderer.color = Color.gray;
        //}
    }
    public void OnHoverEnter()
    {

    }
    public void OnHoverExit()
    {

    }
}