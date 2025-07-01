using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabButtonController : MonoBehaviour
{
    [SerializeField] public List<Button> tabButtons;
    [SerializeField] public List<Image> tabImages;
    
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = Color.gray;

    private int currentSelectedIndex = -1;

    private void Start()
    {
        // 처음 시작할 때 첫 번째 탭을 선택 상태로 설정
        SelectTab(0);
    }

    public void SelectTab(int index)
    {
        if (index == currentSelectedIndex) return;

        currentSelectedIndex = index;

        for (int i = 0; i < tabImages.Count; i++)
        {
            tabImages[i].color = (i == index) ? selectedColor : unselectedColor;
        }

        // 탭에 따라 실제 패널 전환도 여기서 수행할 수 있음
        // 예: SoundPanel.SetActive(index == 0), VideoPanel.SetActive(index == 1) ...
    }
}