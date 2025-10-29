#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// [ReadOnly]가 붙었을 때 인스펙터에서 어떻게 그릴지 정의
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    // GUI를 다시 그리는 함수
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // GUI를 '비활성화' 상태로 만듦
        bool previousGUIState = GUI.enabled;
        GUI.enabled = false;

        // 비활성화된 상태로 프로퍼티 필드를 그림 (회색으로 보임)
        EditorGUI.PropertyField(position, property, label);

        // GUI를 다시 '원래' 상태로 복구
        GUI.enabled = previousGUIState;
    }
}
#endif