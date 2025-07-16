using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 자동 조리 스테이션: 재료를 놓으면 자동으로 조리되고, 완료 시 결과물이 생성됨
/// </summary>
public class AutomaticStation : BaseStation, IInteractable, IPlaceableStation
{
    private void Update()
    {
        if (!timer.IsRunning) return;

        timer.Update(Time.deltaTime);  // 실제 타이머 값 감소

        // UI 갱신
        timerController?.UpdateTimer(timer.Remaining, timer.Duration);

        if (timer.Remaining <= 0f)
        {
            ProcessCookingResult();
            ResetStation();
        }
    }

    /// <summary>
    /// 자동 스테이션은 플레이어와 상호작용하지 않음
    /// </summary>
    public void Interact(PlayerInventory playerInventory, InteractionType interactionType) { }
}