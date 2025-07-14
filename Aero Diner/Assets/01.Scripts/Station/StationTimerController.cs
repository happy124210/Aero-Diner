using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StationTimerController : MonoBehaviour
{
    [SerializeField] private List<Sprite> sprites;

    private Image TimerImg;
    //private int currentIdx = -1;

    private void Awake()
    {
        TimerImg = GetComponent<Image>();
    }

    /// <summary>
    /// 전체 시간 중 현재 남은 시간에 해당하는 스프라이트 인덱스를 계산하여 반환
    /// </summary>
    /// <param name="currentCookingTime">현재 남은 조리 시간</param>
    /// <param name="totalTime">총 조리 시간</param>
    /// <returns>보여줄 스프라이트의 인덱스</returns>
    private int GetSpriteIndex(float currentCookingTime, float totalTime)
    {
        var threshold = totalTime / sprites.Count;

        for (int i = 0; i < sprites.Count; i++)
        {
            var divided = threshold * i;

            if (currentCookingTime <= divided)
            {
                return i;
            }
        }

        return sprites.Count - 1; // 마지막 인덱스 반환
    }

    /// <summary>
    /// 현재 조리 시간에 맞는 스프라이트로 타이머 이미지 업데이트
    /// </summary>
    /// <param name="currentCookingTime">현재 남은 조리 시간</param>
    /// <param name="totalTime">총 조리 시간</param>
    public void UpdateTimer(float currentCookingTime, float totalTime)
    {
        TimerImg.sprite = sprites[GetSpriteIndex(currentCookingTime, totalTime)];
    }
}
