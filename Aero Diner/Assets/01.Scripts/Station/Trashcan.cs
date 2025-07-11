using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trashcan : MonoBehaviour, IInteractable
{
    private OutlineShaderController outline;

    private void Awake()
    {
        outline = GetComponent<OutlineShaderController>();

        string objName = gameObject.name;
        string resourcePath = $"Datas/Station/{objName}Data";

        // SO 로드
        StationData data = Resources.Load<StationData>(resourcePath);
        if (data != null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = data.stationIcon;   // StationData에 있는 아이콘 사용
            }
        }
        else
        {
            Debug.LogWarning($"[IconLoader] 해당 오브젝트 '{objName}'에 대한 StationData를 '{resourcePath}' 경로에서 찾지 못했습니다.");
        }
    }


    public void Interact(PlayerInventory playerInventory, InteractionType interactionType)
    {

    }

    public void OnHoverEnter()
    {
        outline?.EnableOutline();
    }
    public void OnHoverExit()
    {
        outline?.DisableOutline();
    }
}
