using UnityEngine;

public class PlacementManager : Singleton<PlacementManager>
{
    [HideInInspector] public TilemapController tilemapController;
    [SerializeField] private PlayerInventory playerInventory;

    [Header("테스트용 인벤토리")]
    [SerializeField] private TestInventory testInventory;

    private GameObject stationPrefab;                               // 자동 설정됨

    private void Update()
    {
        UpdateStationPrefabFromTestInventory(); // 매 프레임 업데이트 가능, 추후 테스트후 최적화
    }

    /// <summary>
    /// 들고 있는 station을 지정된 그리드셀에 배치 시도
    /// </summary>
    public bool TryPlaceStationAt(GameObject gridCellGO, IMovableStation heldStation)
    {
        if (heldStation == null)
            return false;

        // 자식이 존재하는데 그게 본인이 아니면 배치 금지
        if (gridCellGO.transform.childCount > 0)
        {
            bool isSameObject = false;

            foreach (Transform child in gridCellGO.transform)
            {
                if (child == heldStation.GetTransform())
                {
                    isSameObject = true;
                    break;
                }
            }

            if (!isSameObject)
                return false;
        }

        // 배치 수행
        Transform stationTr = heldStation.GetTransform();
        stationTr.SetParent(gridCellGO.transform);
        stationTr.localPosition = Vector3.zero;

        var rb = stationTr.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = true;

        var col = stationTr.GetComponent<Collider2D>();
        if (col) col.enabled = true;

        tilemapController.UpdateGridCellStates();
        tilemapController.AlignShelvesToGridCells();

        return true; // 배치 성공
    }


    /// <summary>
    /// 현재 들고 있는 설비을 지정된 그리드셀에 배치 시도
    /// </summary>
    public void TestTryPlaceStationAt(GameObject gridCellGO)
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