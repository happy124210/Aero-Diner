using UnityEngine;
using System.Linq;

/// <summary>
/// 플레이어가 상호작용하면 재료를 가공하여 가공된 재료를 생성하는 스테이션
/// 주요 기능:
/// - PlaceIngredient(): 재료 오브젝트를 생성하고, 플레이어가 내려놓은 재료의 데이터를 바탕으로
///   생성할 재료(selectedIngredient)와 가공 허용 재료 그룹(neededIngredients)을 동적으로 채움
/// - Interact(): J 키를 누르는 동안 조리 타이머가 감소하며, 타이머가 다 되면 가공 처리
/// - ProcessIngredient(): 조리가 완료되면 재료 오브젝트를 제거하고 결과 처리를 수행
/// </summary>
public class PassiveStation : BaseStation, IInteractable
{
    [Header("가공 허용 재료 그룹")]
    public FoodData[] neededIngredients;

    /// <summary>
    /// 플레이어가 J 키 등으로 상호작용할 때 호출
    /// InteractionType.Use: 타이머 감소 및 조리 진행
    /// InteractionType.Stop: 조리 중단 및 사운드 정지
    /// </summary>
    public void Interact(PlayerInventory playerInventory, InteractionType interactionType)
    {
        if (interactionType == InteractionType.Use)
        {
            // 조리 조건 검사
            if (!CanStartCooking())
            {
                if (showDebugInfo) Debug.Log("[PassiveStation] 조리 조건 불충분");
                ResetCookingTimer();  // 조건이 부족하면 타이머 리셋
                return;
            }

            // 조리 시작 처리
            if (!isCooking)
            {
                isCooking = true;
                StartCooking(); // BaseStation의 사운드 및 UI 활성화 포함
            }

            // 조리 진행 처리
            timer.Update(Time.deltaTime);
            timerController?.UpdateTimer(timer.Remaining, timer.Duration);

            // 조리 완료 시 결과 생성 및 초기화
            if (timer.Remaining <= 0f)
            {
                ProcessCookingResult();
                ResetStation();
            }
        }
        else if (interactionType == InteractionType.Stop)
        {
            // 플레이어가 J 키에서 손을 뗀 경우: 조리 중단
            isCooking = false;
            var sfx = StationSFXResolver.GetSFXFromStationData(stationData);
            EventBus.StopLoopSFX(sfx);
            if (showDebugInfo) Debug.Log("[PassiveStation] 조리 중단됨");
        }
    }

    /// <summary>
    /// 현재 재료 목록이 조리 가능한 완전한 레시피를 충족하는지 확인
    /// </summary>
    /// <returns>조리 가능 여부</returns>
    private bool CanStartCooking()
    {
        return cookedIngredient != null &&
               cookedIngredient.ingredients.All(id => currentIngredients.Contains(id));
    }

    /// <summary>
    /// 조리 타이머를 초기화하고 UI를 갱신
    /// </summary>
    private void ResetCookingTimer()
    {
        timer = new CookingTimer(cookingTime);
        timerController?.UpdateTimer(timer.Remaining, timer.Duration);
        timerController?.gameObject.SetActive(false);
    }
}