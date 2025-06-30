using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlaceableStation
{
    /// <summary>
    /// 재료가 스테이션에 내려놓였을 때 호출되는 함수.
    /// </summary>
    /// <param name="data">내려놓는 재료 데이터</param>
    void PlaceIngredient(ScriptableObject data);
}
