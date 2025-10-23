using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTextureHandler : MonoBehaviour
{
    public RenderTexture UnitRT { get; private set; }
    PlayerUnit unit;
    Camera renderCam;
    LayerMask LayerMask;
    int layerForAnimation;
    private void Awake()
    {
        LayerMask = LayerMask.GetMask("Animation");
        layerForAnimation = LayerMask.NameToLayer("Animation");
        // 렌더 텍스처 세팅
        UnitRT = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
        UnitRT.filterMode = FilterMode.Point;
        UnitRT.Create();
        // 카메라 세팅
        // 카메라 오브젝트 생성
        GameObject camGO = new GameObject("IconRenderCamera_");
        camGO.transform.SetParent(gameObject.transform);
        camGO.transform.localPosition = new Vector3(0, 0.8f, -10);
        // 카메라 컴포넌트 추가 및 설정
        renderCam = camGO.AddComponent<Camera>();
        renderCam.orthographic = true;
        renderCam.orthographicSize = 1.3f;
        renderCam.targetTexture = UnitRT;
        renderCam.cullingMask = LayerMask;
        renderCam.clearFlags = CameraClearFlags.SolidColor; // 배경색으로 클리어
        renderCam.backgroundColor = new Color(0, 0, 0, 0); // 완전 투명 배경
        renderCam.nearClipPlane = 0.0f;
        renderCam.farClipPlane = 20.0f;
        renderCam.allowHDR = false;
        renderCam.allowMSAA = false;
    }
    private void OnDestroy()
    {
        UnitRT.Release();
        Destroy(UnitRT);
        if(renderCam) Destroy(renderCam.gameObject);
        if(unit) Destroy(unit.gameObject);
    }
    
    public void Init(PoolType type)
    {
        // 랜터 텍스처용 유닛 오브젝트 세팅
        GameObject unitGO = ObjectPoolManager.Instance.Get(type);
        unitGO.transform.position = gameObject.transform.position;
        unitGO.transform.Find("HpBar").gameObject.SetActive(false);
        GameObject unitRootGO = unitGO.transform.Find("UnitRoot")?.gameObject;
        // nitRootGO = null이면 말 유닛일 가능성 있음
        if (unitRootGO == null)
        {
            unitRootGO = unitGO.transform.Find("HorseRoot")?.gameObject;
            // 그래도 없다면 오류
            if (unitRootGO == null) { Debug.LogError("UnitRoot, HorseRoot 둘다 없음"); return; }
            // 말 유닛은 카메라 위치 다르게 설정
            Vector3 localPos = unitRootGO.transform.localPosition;
            localPos.x -= 0.2f; // 말 유닛은 약간 왼쪽으로 보정
            unitRootGO.transform.localPosition = localPos;
            renderCam.gameObject.transform.localPosition = new Vector3(0, 1.4f, -10);
            renderCam.orthographicSize = 1.8f;
        }
        //unitRootGO.layer = layerForAnimation;
        //SetLayerToAllTransform(unitRootGO, layerForAnimation); // 251023: 미리 레이어 세팅으로 변경
        unitRootGO.transform.Find("Shadow").gameObject.SetActive(false);
        PlayerUnit playerUnit = unitGO.GetComponent<PlayerUnit>();
        playerUnit.SetForRenderTexture();
        unit = playerUnit;
    }
    public void SetCanSpawnUnit(bool canSpawn)
    {
        unit.UnitController.Animator.SetFloat(
                unit.AnimationData.SpeedParameterHash,
                canSpawn ? 1f : 0f);
    }
    private void SetLayerToAllTransform(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerToAllTransform(child.gameObject, layer);
        }
    }
}
