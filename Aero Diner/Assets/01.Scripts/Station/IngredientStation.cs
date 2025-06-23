using UnityEngine;

/// <summary>
/// 플레이어가 상호작용하면 재료를 생성해주는 스테이션
/// </summary>
public class IngredientStation : MonoBehaviour, IInteractable
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
    public void Interact(PlayerInventory playerInventory)
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
    }
}