using UnityEngine;
using UnityEditor;

// 이 스크립트는 SpriteCapture 컴포넌트의 인스펙터를 커스터마이징합니다.
// 반드시 "Editor" 폴더 안에 있어야 합니다.
[CustomEditor(typeof(SpriteCapture))]
public class SpriteCaptureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 필드들(targetObject, cameraSize 등)을 그립니다.
        DrawDefaultInspector();

        EditorGUILayout.Space();

        // SpriteCapture 스크립트 인스턴스를 가져옵니다.
        SpriteCapture captureScript = (SpriteCapture)target;

        // 버튼을 추가합니다.
        if (GUILayout.Button("Capture Sprite", GUILayout.Height(30)))
        {
            // 버튼이 클릭되면 캡처 함수를 호출합니다.
            captureScript.CaptureSprite();
        }
    }
}