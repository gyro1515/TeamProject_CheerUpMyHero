using UnityEditor;
using UnityEngine;

// 이 스크립트는 에셋이 임포트될 때 자동으로 특정 설정을 적용합니다.
// 반드시 "Editor" 폴더 안에 있어야 합니다.
public class SpriteImportProcessor : AssetPostprocessor
{
    // 텍스처가 임포트되기 직전에 호출됩니다.
    void OnPreprocessTexture()
    {
        // SpriteCapture 스크립트에서 설정한 기본 저장 경로
        // SpriteCapture.cs의 'savePath'와 이 경로를 일치시켜야 합니다.
        string targetPath = "Assets/Resources/UnitIcon";

        if (assetPath.StartsWith(targetPath))
        {
            Debug.Log($"자동 스프라이트 임포터 실행: {assetPath}");

            TextureImporter textureImporter = (TextureImporter)assetImporter;

            // 텍스처 타입을 스프라이트로 변경합니다.
            textureImporter.textureType = TextureImporterType.Sprite;
        }
    }
}