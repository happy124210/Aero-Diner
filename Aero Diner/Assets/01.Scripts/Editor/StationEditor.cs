#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;

/// <summary>
/// UpdateRecipePreview() 메서드를 가진 컴포넌트에 버튼 자동 제공
/// </summary>
[CustomEditor(typeof(MonoBehaviour), true)]
public class StationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MonoBehaviour mono = (MonoBehaviour)target;
        MethodInfo previewMethod = mono.GetType().GetMethod("UpdateRecipePreview", BindingFlags.Public | BindingFlags.Instance);

        if (previewMethod != null)
        {
            GUILayout.Space(10);
            if (GUILayout.Button("레시피 미리보기"))
            {
                previewMethod.Invoke(mono, null);
            }
        }
    }
}
#endif