#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// 레시피 프리뷰 버튼 및 matchedRecipeNames 출력 에디터
/// </summary>
[CustomEditor(typeof(MonoBehaviour), true)]
public class StationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MonoBehaviour mono = (MonoBehaviour)target;
        System.Type type = mono.GetType();

        MethodInfo previewMethod = type.GetMethod("UpdateRecipePreview", BindingFlags.Public | BindingFlags.Instance);
        FieldInfo matchedField = type.GetField("matchedRecipeNames", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        GUILayout.Space(10);
        if (previewMethod != null && GUILayout.Button("레시피 미리보기"))
        {
            previewMethod.Invoke(mono, null);
        }

        if (matchedField != null && matchedField.GetValue(mono) is HashSet<string> idSet)
        {
            GUILayout.Space(10);
            GUILayout.Label("📋 유효 재료 ID 목록", EditorStyles.boldLabel);

            if (idSet.Count == 0)
            {
                GUILayout.Label("없음");
            }
            else
            {
                foreach (var id in idSet)
                {
                    GUILayout.Label($"🧂 {id}");
                }
            }
        }
    }
}
#endif