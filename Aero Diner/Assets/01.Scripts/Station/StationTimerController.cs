using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StationTimerController : MonoBehaviour
{
    [SerializeField] private List<Sprite> sprites;

    private Image TimerImg;
    private int currentIdx = -1;

    private void Awake()
    {
        TimerImg = GetComponent<Image>();
        //gameObject.SetActive(false); // 처음에는 비활성화 상태로 시작

    }

    /// <summary>
    /// 전체 시간(totalime)중에 보여줘야 하는 스프라이트의 인덱스 값을 반환
    /// </summary>
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

    public void UpdateTimer(float currentCookingTime, float totalTime)
    {
        TimerImg.sprite = sprites[GetSpriteIndex(currentCookingTime, totalTime)];
    }
}
