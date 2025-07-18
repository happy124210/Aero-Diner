using UnityEditor;
using UnityEngine;

/// <summary>
/// TilemapController의 커스텀 인스펙터
/// 버튼을 눌러 셀을 보이게 할 수 있음
/// </summary>
[CustomEditor(typeof(TilemapController))]
public class TilemapControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TilemapController controller = (TilemapController)target;

        GUILayout.Space(10);
        if (GUILayout.Button("셀 보이기 "))
        {
            controller.ShowAllCells();
        }
    }
}