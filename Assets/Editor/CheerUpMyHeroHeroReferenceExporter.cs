#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor-only source-reference exporter for the three production-approved heroes
/// whose independent image sets are still missing in Drive.
///
/// This tool never changes the prefabs. It instantiates each source prefab in a
/// temporary preview scene and exports a transparent PNG from the actual composed
/// SpriteRenderer hierarchy so art production can use a factual visual reference.
/// </summary>
public static class CheerUpMyHeroHeroReferenceExporter
{
    private const int OutputSize = 1024;
    private const float PaddingMultiplier = 1.18f;
    private const string OutputFolder = "Assets/SourceReferenceExports";

    private readonly struct HeroSource
    {
        public readonly int UnitIndex;
        public readonly string Name;
        public readonly string PrefabPath;

        public HeroSource(int unitIndex, string name, string prefabPath)
        {
            UnitIndex = unitIndex;
            Name = name;
            PrefabPath = prefabPath;
        }
    }

    private static readonly HeroSource[] Sources =
    {
        new HeroSource(120003, "Nayet", "Assets/Prefabs/Unit/Unit_120003.prefab"),
        new HeroSource(125001, "Peila", "Assets/Prefabs/Unit/Unit_125001.prefab"),
        new HeroSource(125003, "Mireil", "Assets/Prefabs/Unit/Unit_125003.prefab"),
    };

    [MenuItem("CheerUpMyHero/Art QA/Export Nayet Peila Mireil Source References")]
    public static void ExportAll()
    {
        EnsureOutputFolder();

        var exported = new List<string>();
        foreach (var source in Sources)
        {
            try
            {
                string path = Export(source);
                exported.Add(path);
                Debug.Log($"[HeroReferenceExporter] {source.UnitIndex} {source.Name}: {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[HeroReferenceExporter] Failed {source.UnitIndex} {source.Name}: {e}");
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog(
            "CheerUpMyHero Source Reference Export",
            $"Exported {exported.Count}/{Sources.Length} source-reference PNGs.\n\n" +
            string.Join("\n", exported) +
            "\n\nThese are factual prefab renders for reference only. They do not modify runtime prefabs.",
            "OK");
    }

    private static string Export(HeroSource source)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(source.PrefabPath);
        if (prefab == null)
            throw new FileNotFoundException("Prefab not found", source.PrefabPath);

        Scene previewScene = EditorSceneManager.NewPreviewScene();
        RenderTexture rt = null;
        Texture2D texture = null;

        try
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, previewScene) as GameObject;
            if (instance == null)
                throw new InvalidOperationException("Prefab instantiate failed: " + source.PrefabPath);

            instance.transform.position = Vector3.zero;

            // Evaluate the prefab's current Animator state once without altering its asset.
            foreach (Animator animator in instance.GetComponentsInChildren<Animator>(true))
            {
                if (animator.runtimeAnimatorController == null) continue;
                animator.Rebind();
                animator.Update(0f);
            }

            SpriteRenderer[] renderers = instance.GetComponentsInChildren<SpriteRenderer>(true);
            bool hasBounds = false;
            Bounds bounds = default;
            foreach (SpriteRenderer renderer in renderers)
            {
                if (!renderer.enabled || renderer.sprite == null) continue;
                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds)
                throw new InvalidOperationException("No enabled SpriteRenderer with sprite found in " + source.PrefabPath);

            GameObject cameraGo = new GameObject("SourceReferenceCamera");
            SceneManager.MoveGameObjectToScene(cameraGo, previewScene);
            Camera camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            camera.allowHDR = false;
            camera.allowMSAA = true;

            float halfHeight = Mathf.Max(bounds.extents.y, bounds.extents.x) * PaddingMultiplier;
            camera.orthographicSize = Mathf.Max(halfHeight, 0.1f);
            camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, bounds.center.z - 20f);
            camera.transform.rotation = Quaternion.identity;

            rt = new RenderTexture(OutputSize, OutputSize, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 4
            };
            rt.Create();
            camera.targetTexture = rt;
            camera.Render();

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            texture = new Texture2D(OutputSize, OutputSize, TextureFormat.RGBA32, false, false);
            texture.ReadPixels(new Rect(0, 0, OutputSize, OutputSize), 0, 0, false);
            texture.Apply(false, false);
            RenderTexture.active = previous;

            string outputPath = $"{OutputFolder}/{source.UnitIndex}_{source.Name}_PrefabReference_v0.1.png";
            File.WriteAllBytes(outputPath, texture.EncodeToPNG());
            return outputPath;
        }
        finally
        {
            if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
            if (rt != null)
            {
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }
            if (previewScene.IsValid()) EditorSceneManager.ClosePreviewScene(previewScene);
        }
    }

    private static void EnsureOutputFolder()
    {
        if (AssetDatabase.IsValidFolder(OutputFolder)) return;
        if (!AssetDatabase.IsValidFolder("Assets/SourceReferenceExports"))
            AssetDatabase.CreateFolder("Assets", "SourceReferenceExports");
    }
}
#endif
