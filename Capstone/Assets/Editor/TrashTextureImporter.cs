using UnityEditor;
using UnityEngine;

namespace DigGame.Editor
{
    /// <summary>외부 픽셀 아트가 흐려지지 않도록 임포트 품질을 고정한다.</summary>
    public class TrashTextureImporter : AssetPostprocessor
    {
        const string TrashFolder = "Assets/Resources/TrashSprites/";
        const string EnvironmentFolder = "Assets/Resources/EnvironmentPixel/";
        const string PixelRefreshFolder = "Assets/Resources/PixelRefresh/";

        void OnPreprocessTexture()
        {
            bool isTrash = assetPath.StartsWith(TrashFolder, System.StringComparison.OrdinalIgnoreCase);
            bool isEnvironment = assetPath.StartsWith(EnvironmentFolder, System.StringComparison.OrdinalIgnoreCase);
            bool isPixelRefresh = assetPath.StartsWith(PixelRefreshFolder, System.StringComparison.OrdinalIgnoreCase);
            if (!isTrash && !isEnvironment && !isPixelRefresh) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = isPixelRefresh;
        }
    }
}
