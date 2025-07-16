using UnityEngine;

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
    /// </summary>
    public void Interact(PlayerInventory playerInventory, InteractionType interactionType)
    {
        //// 재료가 없는 경우 조리 타이머 초기화 후 종료
        //if (currentIngredients.Count == 0)
        //{
        //    currentCookingTime = cookingTime;
        //    if (showDebugInfo) Debug.Log("재료가 없어 조리 타이머가 리셋되었습니다.");
        //    return;
        //}

        //// 필요한 재료 그룹이 지정되어 있다면 유효성 검사
        //if (neededIngredients.Any())
        //{
        //    // 허용되지 않은 재료가 들어있는지 검사
        //    bool hasInvalid = currentIngredients.Any(id => !IsIngredientIDAllowed(id));
        //    if (hasInvalid)
        //    {
        //        if (showDebugInfo) Debug.Log("요구된 재료가 아닌 항목이 포함되어 있어 타이머가 리셋됨.");
        //        currentCookingTime = cookingTime;
        //        return;
        //    }

        //    // 모든 필요한 재료가 있는지 검사
        //    List<string> neededIds = neededIngredients.Select(n => n.id).ToList();
        //    bool allRequiredPresent = neededIds.All(id => currentIngredients.Contains(id));
        //    if (!allRequiredPresent)
        //    {
        //        if (showDebugInfo) Debug.Log("모든 재료가 준비되지 않아 조리가 시작되지 않습니다.");
        //        currentCookingTime = cookingTime;
        //        return;
        //    }
        //}

        //// 실제 상호작용이 'Use'일 때만 처리
        //if (interactionType == InteractionType.Use)
        //{
        //    // 조리 중이 아니면 이제 시작
        //    if (!isCooking)
        //    {
        //        if (stationData != null && stationData.workType == WorkType.Passive)
        //        {
        //            var sfx = StationSFXResolver.GetSFXFromStationData(stationData);
        //            EventBus.PlayLoopSFX(SFXType.PlayCooking);
        //        }

        //        // 기존에 저장된 시간이 없으면만 초기화
        //        if (Mathf.Approximately(currentCookingTime, cookingTime) || currentCookingTime <= 0f)
        //        {
        //            currentCookingTime = cookingTime;
        //        }

        //        isCooking = true;
        //        StartCooking(); // 이 함수 내부도 currentCookingTime 초기화하는 부분이 있다면 제거
        //        return;
        //    }

        //    // 이미 조리 중이면 진행
        //    currentCookingTime -= Time.deltaTime;
        //    UpdateCookingProgress();

        //    if (currentCookingTime <= 0f)
        //    {
        //        ProcessCookingResult();
        //        currentIngredients.Clear();
        //        currentCookingTime = cookingTime;
        //    }
        //}
        //else
        //{
        //    // J키에서 손을 뗀 경우
        //    if (interactionType == InteractionType.Stop)
        //    {
        //        isCooking = false; // 시간 정지
        //        EventBus.StopLoopSFX(); // 사운드 정지
        //        if (showDebugInfo) Debug.Log("[PassiveStation] 조리 중단됨");
        //    }
        //}
    }


    //private bool IsIngredientIDAllowed(string id)
    //{
    //    if (neededIngredients == null || !neededIngredients.Any())
    //        return true;

    //    return neededIngredients.Any(entry => entry.id == id);
    //}
}