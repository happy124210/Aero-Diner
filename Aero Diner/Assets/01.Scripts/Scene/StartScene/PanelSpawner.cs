using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventBus.Raise(UIEventType.ShowPressAnyKey);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
