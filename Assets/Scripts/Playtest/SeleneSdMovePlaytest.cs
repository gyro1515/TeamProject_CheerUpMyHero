using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Runtime-only playtest overlay for Selene(Unit_125004).
// It never replaces the original prefab art on disk; it only hides the existing
// SpriteRenderers while the object is alive and restores them when removed.
public sealed class SeleneSdMovePlaytest : MonoBehaviour
{
    private const string ResourcePath = "Playtest/Selene/Selene_SD_Move_Playtest_v1_1";
    private const int FrameCount = 6;
    private const float Fps = 12f;
    private const float PixelsPerUnit = 256f;

    private readonly List<SpriteRenderer> _originalRenderers = new List<SpriteRenderer>();
    private readonly List<bool> _originalEnabled = new List<bool>();
    private SpriteRenderer _playtestRenderer;
    private Sprite[] _frames;
    private Vector3 _lastWorldPosition;
    private float _frameTimer;
    private int _frameIndex;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        var installer = new GameObject("SeleneSdMovePlaytestInstaller");
        DontDestroyOnLoad(installer);
        installer.AddComponent<Installer>();
    }

    private sealed class Installer : MonoBehaviour
    {
        private readonly HashSet<int> _patched = new HashSet<int>();

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartCoroutine(ScanLoop());
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ScanNow();
        }

        private IEnumerator ScanLoop()
        {
            var wait = new WaitForSeconds(0.5f);
            while (true)
            {
                ScanNow();
                yield return wait;
            }
        }

        private void ScanNow()
        {
            var all = Object.FindObjectsOfType<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t == null || t.name != "Unit_125004") continue;
                int id = t.gameObject.GetInstanceID();
                if (_patched.Contains(id)) continue;
                if (t.GetComponent<SeleneSdMovePlaytest>() == null)
                    t.gameObject.AddComponent<SeleneSdMovePlaytest>();
                _patched.Add(id);
            }
        }
    }

    private void Awake()
    {
        Texture2D sheet = Resources.Load<Texture2D>(ResourcePath);
        if (sheet == null)
        {
            Debug.LogError("[Selene SD Playtest] texture not found: " + ResourcePath);
            enabled = false;
            return;
        }

        if (sheet.width != 3072 || sheet.height != 512)
        {
            Debug.LogError($"[Selene SD Playtest] unexpected sheet size {sheet.width}x{sheet.height}; expected 3072x512");
            enabled = false;
            return;
        }

        _frames = new Sprite[FrameCount];
        for (int i = 0; i < FrameCount; i++)
        {
            var rect = new Rect(i * 512, 0, 512, 512);
            _frames[i] = Sprite.Create(sheet, rect, new Vector2(0.5f, 0.08f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
            _frames[i].name = $"Selene_Move_{i + 1:00}_Playtest";
        }

        Bounds originalBounds = default;
        bool hasBounds = false;
        int highestOrder = 0;
        string sortingLayer = "Default";

        var renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in renderers)
        {
            if (r == null) continue;
            _originalRenderers.Add(r);
            _originalEnabled.Add(r.enabled);
            if (r.sprite != null)
            {
                if (!hasBounds)
                {
                    originalBounds = r.bounds;
                    hasBounds = true;
                }
                else originalBounds.Encapsulate(r.bounds);
            }
            if (r.sortingOrder >= highestOrder)
            {
                highestOrder = r.sortingOrder;
                sortingLayer = r.sortingLayerName;
            }
            r.enabled = false;
        }

        var go = new GameObject("Selene_SD_PlaytestRenderer");
        go.transform.SetParent(transform, false);
        _playtestRenderer = go.AddComponent<SpriteRenderer>();
        _playtestRenderer.sprite = _frames[0];
        _playtestRenderer.sortingLayerName = sortingLayer;
        _playtestRenderer.sortingOrder = highestOrder + 100;

        float targetHeight = hasBounds && originalBounds.size.y > 0.01f ? originalBounds.size.y : 1.8f;
        float sourceHeight = _frames[0].bounds.size.y;
        float scale = sourceHeight > 0f ? targetHeight / sourceHeight : 1f;
        go.transform.localScale = Vector3.one * scale;

        if (hasBounds)
            go.transform.localPosition = transform.InverseTransformPoint(originalBounds.center);

        _lastWorldPosition = transform.position;
        Debug.Log($"[Selene SD Playtest] applied to {name}, 6 frames @ {Fps}fps, scale={scale:F3}");
    }

    private void Update()
    {
        if (_playtestRenderer == null || _frames == null) return;

        Vector3 now = transform.position;
        float dx = now.x - _lastWorldPosition.x;
        if (Mathf.Abs(dx) > 0.0001f)
            _playtestRenderer.flipX = dx < 0f;
        _lastWorldPosition = now;

        _frameTimer += Time.deltaTime;
        float step = 1f / Fps;
        while (_frameTimer >= step)
        {
            _frameTimer -= step;
            _frameIndex = (_frameIndex + 1) % FrameCount;
            _playtestRenderer.sprite = _frames[_frameIndex];
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _originalRenderers.Count && i < _originalEnabled.Count; i++)
        {
            if (_originalRenderers[i] != null)
                _originalRenderers[i].enabled = _originalEnabled[i];
        }
    }
}
