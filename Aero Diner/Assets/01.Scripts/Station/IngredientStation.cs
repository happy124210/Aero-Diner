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
        // 필요한 컴포넌트나 데이터가 누락된 경우 실행하지 않음
        if (ingredientGroup == null || selectedIngredient == null || spawnPoint == null) return;

        // 선택된 재료 데이터에 해당하는 프리팹을 가져옴
        GameObject prefab = ingredientGroup.GetPrefabByData(selectedIngredient);

        // 프리팹이 존재하면 스폰 위치에 재료 인스턴스를 생성
        if (prefab != null)
        {
            Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        }

        if (selectedIngredient == null || spawnPoint == null)
        {
            Debug.LogError("선택된 음식 데이터 또는 생성 위치가 설정되지 않았습니다.");
            return;
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