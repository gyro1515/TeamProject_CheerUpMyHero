using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

// **********************사용법***********************
// 1. Z_CaptureSpriteScene 씬열기
// 2. 캡처할 오브젝트들 씬에 배치하기
// 3. 오브젝트들 인스펙터창(SpriteCaptureHandler)의 targetObjects에 넣기
// 4. 카메라 사이즈 오브젝트에 맞게 조정하기
// 5. 배치 간격 설정하기 -> spacing 값 설정
// 5. 캡처할 레이어 이름 설정하기 (기본값: "Animation")
// 7. SpriteCaptureHandler에서 "CaptureSprite" 버튼 클릭하기
// ***************************************************

// (런타임 기능이 제거된 에디터 전용 버전)
public class SpriteCapture : MonoBehaviour
{
    [Header("Capture Target")]
    [Tooltip("캡처할 대상 (이 오브젝트를 중심으로 카메라가 배치됨)")]
    public List<Transform> targetObjects = new List<Transform>();

    [Tooltip("자동 배치 간격")]
    public Vector3 spacing = new Vector3(5.0f, 0, 0); 

    [Tooltip("캡처할 대상이 있는 레이어 이름")]
    public string captureLayerName = "Animation";

    [Header("File Settings")]
    [Tooltip("저장될 스프라이트의 해상도")]
    public Vector2Int resolution = new Vector2Int(512, 512);

    [Tooltip("저장 경로 (Assets/ 기준)")]
    public string savePath = "Resources/UnitIcon";

    [Tooltip("캡처에 사용할 카메라 (씬에 미리 배치된 카메라를 사용!)")]
    public Camera captureCamera;


    // 에디터 스크립트에서 이 함수를 호출하여 캡처를 실행합니다.
    public void CaptureSprite()
    {
        // 1. 유효성 검사 (Layer)
        int layer = LayerMask.NameToLayer(captureLayerName);
        if (layer == -1)
        {
            Debug.LogError($"'{captureLayerName}' 레이어를 찾을 수 없습니다. Edit > Project Settings > Tags and Layers에서 레이어를 먼저 생성해주세요.");
            return;
        }
        if (targetObjects.Count == 0)
        {
            Debug.LogError("캡처할 대상(Target Object)이 설정되지 않았습니다! 인스펙터에 캡처할 오브젝트 넣어주세요(오브젝트는 씬에 존재해야 합니다)");
            return;
        }
        if(captureCamera == null)
        {
            Debug.LogError("캡처에 사용할 카메라가 설정되지 않았습니다! 인스펙터에 캡처용 카메라 오브젝트 넣어주세요(카메라는 씬에 존재해야 합니다)");
            return;
        }

        // 오브젝트들 나열하기
        for (int i = 0; i < targetObjects.Count; i++)
        {
            if (targetObjects[i] != null)
            {
                targetObjects[i].position = spacing * i;
            }
        }

        // 2. 캡처 카메라용 임시 GameObject 생성
        //GameObject camGO = new GameObject("TempCaptureCamera");
        // 3. 카메라 설정 세팅
        //Camera captureCamera = camGO.AddComponent<Camera>();
        captureCamera.orthographic = true;
        captureCamera.clearFlags = CameraClearFlags.SolidColor;
        captureCamera.backgroundColor = new Color(0, 0, 0, 0); // 투명 배경
        captureCamera.cullingMask = 1 << layer; // 지정된 레이어만
        captureCamera.enabled = false; // 수동 렌더링

        foreach (var target in targetObjects)
        {
            if (target != null)
            {
                // 각 대상에 대해 캡처 수행
                Capture(target, captureCamera);
            }
        }

        // 10. 유니티 에디터가 새 파일을 인식하도록 강제 새로고침
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
    void Capture(Transform targetObject, Camera captureCamera)
    {
        // 3. 카메라 위치 세팅
        captureCamera.gameObject.transform.position = new Vector3(targetObject.position.x, captureCamera.gameObject.transform.position.y, captureCamera.gameObject.transform.position.z);

        // 4. 렌더 텍스처 생성
        RenderTexture rt = new RenderTexture(resolution.x, resolution.y, 24);
        captureCamera.targetTexture = rt;
        RenderTexture.active = rt;

        // 5. 카메라 수동 렌더링
        captureCamera.Render();

        // 6. 픽셀 복사
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        // 7. PNG 인코딩
        byte[] bytes = tex.EncodeToPNG();

        // 8. 파일 저장 (Assets 폴더 기준)
        string fullPath = Path.Combine(Application.dataPath, savePath);
        Directory.CreateDirectory(fullPath);
        string filePath = Path.Combine(fullPath, targetObject.gameObject.name + ".png");
        File.WriteAllBytes(filePath, bytes);

        // 9. 리소스 정리 (에디터 모드이므로 DestroyImmediate 사용)
        RenderTexture.active = null;
        captureCamera.targetTexture = null;

        DestroyImmediate(rt);
        DestroyImmediate(tex);
        //DestroyImmediate(camGO); // 임시 카메라 GameObject 제거

        Debug.Log($"스프라이트 저장 완료: {filePath}");
    }
}