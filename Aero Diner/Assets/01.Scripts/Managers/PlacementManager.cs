using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    [HideInInspector] public TilemapController tilemapController;
    [SerializeField] private PlayerInventory playerInventory;

    [Header("테스트용 인벤토리")]
    [SerializeField] private TestInventory testInventory;

    private GameObject stationPrefab;                               // 자동 설정됨

    private void Update()
    {
        UpdateStationPrefabFromPlayer();        // 플레이어가 들고 있는 Station 오브젝트를 프리팹으로 등록
        UpdateStationPrefabFromTestInventory(); // 매 프레임 업데이트 가능, 추후 테스트후 최적화
    }

    /// <summary>
    /// 플레이어가 들고 있는 Station 오브젝트를 프리팹으로 등록
    /// </summary>
    private void UpdateStationPrefabFromPlayer()
    {
        if (playerInventory == null)
        {
            stationPrefab = null;
            return;
        }

        IMovableStation heldStation = playerInventory.heldStation;

        if (heldStation != null)
        {
            Transform stationTransform = heldStation.GetTransform();
            GameObject stationObj = stationTransform.gameObject;

            if (stationObj.CompareTag("Station"))
            {
                stationPrefab = stationObj;
            }
            else
            {
                stationPrefab = null;
            }
        }
        else
        {
            stationPrefab = null;
        }
    }

    /// <summary>
    /// 현재 들고 있는 stationPrefab을 지정된 그리드셀에 배치 시도
    /// </summary>
    public void TryPlaceStationAt(GameObject gridCellGO)
    {
        if (stationPrefab == null) return;

        if (gridCellGO.transform.childCount == 0)
        {
            Instantiate(stationPrefab, gridCellGO.transform);
            tilemapController.UpdateGridCellStates();
        }
    }

    /// <summary>
    /// 테스트 인벤토리에서 Station 태그를 가진 오브젝트를 프리팹으로 등록
    /// </summary>
    private void UpdateStationPrefabFromTestInventory()
    {
        GameObject heldItem = testInventory.GetHeldItem();

        if (heldItem != null && heldItem.CompareTag("Station"))
        {
            stationPrefab = heldItem;
        }
        else
        {
            stationPrefab = null;
        }
    }

#if UNITY_EDITOR
    [SerializeField] public GameObject testTargetGridCell;
#endif
}