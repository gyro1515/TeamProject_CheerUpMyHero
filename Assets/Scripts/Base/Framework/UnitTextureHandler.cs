using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTextureHandler : MonoBehaviour
{
    public RenderTexture UnitRT { get; private set; }
    PlayerUnit unit;
    Camera renderCam;
    LayerMask LayerMask = LayerMask.GetMask("Animation");
    private void Awake()
    {
        // 렌더 텍스처 세팅
        UnitRT = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
        UnitRT.filterMode = FilterMode.Point;
        UnitRT.Create();
        // 카메라 세팅
        GameObject camGO = new GameObject("IconRenderCamera_");
        camGO.transform.SetParent(gameObject.transform);
        camGO.transform.localPosition = new Vector3(0, 0, -10);
        renderCam = camGO.AddComponent<Camera>();
        renderCam.targetTexture = UnitRT;
        renderCam.cullingMask = LayerMask;
        renderCam.clearFlags = CameraClearFlags.SolidColor; // 배경색으로 클리어
        renderCam.backgroundColor = new Color(0, 0, 0, 0); // 완전 투명 배경
        renderCam.nearClipPlane = 0.0f;
        renderCam.farClipPlane = 20.0f;
        renderCam.orthographicSize = 1.5f;
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
        unitGO.transform.Find("HpBar").gameObject.SetActive(false);
        GameObject unitRootGO = unitGO.transform.Find("UnitRoot").gameObject;
        SetLayerToAllTransform(unitRootGO, LayerMask);
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
