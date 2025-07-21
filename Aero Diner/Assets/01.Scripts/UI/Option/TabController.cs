using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabController : MonoBehaviour
{
    [SerializeField] public List<Button> tabButtons;
    [SerializeField] public List<Image> tabImages;
    
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = Color.gray;

    private int currentSelectedIndex = -1;
    public int CurrentIndex => currentSelectedIndex;
    private void Start()
    {
        RequestSelectTab(0);               // 0번 탭을 예약하고
        ApplyTabSelectionVisuals();       // 실제로 색상을 반영
    }

    public void RequestSelectTab(int index)
    {
        currentSelectedIndex = index; // 예약만 해둠
    }

    // 실제 탭이 보여질 때 호출
    public void ApplyTabSelectionVisuals()
    {
        for (int i = 0; i < tabImages.Count; i++)
        {
            tabImages[i].color = (i == currentSelectedIndex) ? selectedColor : unselectedColor;
        }
    }
}