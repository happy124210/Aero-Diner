using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 에디터에서 직접 배치한 GridCell들을 자동으로 탐색하고 관리
/// 자식 오브젝트 중 GridCell 태그를 가진 것만 인식
/// 처음에는 셀을 숨기고, 버튼 클릭 시 보이게 설정 가능
/// </summary>
public class TilemapController : MonoBehaviour
{
    public List<GameObject> gridCells = new List<GameObject>();

    [SerializeField] protected bool showDebugInfo;

    [SerializeField] private Material baseMaterial;
    [SerializeField] private Material placeableMaterial;
    [SerializeField] private Material notPlaceableMaterial;

    private void Awake()
    {
        FindGridCells();
        HideAllCells();                               // 시작 시 셀 숨김
    }

    /// <summary>
    /// 자식 오브젝트 중 GridCell 태그를 가진 오브젝트를 모두 수집
    /// </summary>
    private void FindGridCells()
    {
        gridCells.Clear();

        foreach (Transform child in transform)
        {
            if (child.CompareTag("GridCell"))
            {
                gridCells.Add(child.gameObject);
            }
        }

        if (showDebugInfo) Debug.Log($"GridCell {gridCells.Count}개를 찾았습니다.");
    }

    /// <summary>
    /// 모든 셀의 SpriteRenderer를 비활성화하여 셀을 숨김
    /// </summary>
    public void HideAllCells()
    {
        foreach (var cell in gridCells)
        {
            // SpriteRenderer 비활성화
            var sr = cell.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = false;

                // baseMaterial로 원복
                if (baseMaterial != null)
                    sr.material = baseMaterial;
            }
        }
    }

    /// <summary>
    /// 모든 셀의 SpriteRenderer를 활성화하여 셀을 보이게 함
    /// </summary>
    public void ShowAllCells()
    {
        foreach (var cell in gridCells)
        {
            var sr = cell.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;

                if (baseMaterial != null)
                    sr.material = baseMaterial;
            }
        }
    }

    /// <summary>
    /// 모든 GridCell의 자식 중에 Shelf가 있으면 하단 중심이 겹치도록 정렬
    /// </summary>
    public void AlignShelvesToGridCells()
    {
        foreach (var cell in gridCells)
        {
            foreach (Transform child in cell.transform)
            {
                if (child.CompareTag("Station")) 
                {
                    child.localPosition = Vector3.zero;

                    if (showDebugInfo)
                    {
                        Debug.Log($"Shelf 위치 정렬됨: {child.name} @ {cell.name}");
                    }
                }
            }
        }
    }

    public void UpdateGridCellStates()
    {
        foreach (var cell in gridCells)
        {
            var status = cell.GetComponent<GridCellStatus>();
            if (status != null)
            {
                status.SetMaterials(placeableMaterial, notPlaceableMaterial);
                status.UpdateStatus();
            }
        }
    }
}