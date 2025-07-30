using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tu7 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCloseBtn()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        EventBus.Raise(UIEventType.HideInventory);
        UIEventCaller.CallUIEvent("tu8");
    }
}
