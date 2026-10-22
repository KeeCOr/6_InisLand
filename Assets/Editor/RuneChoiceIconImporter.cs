using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public sealed class RuneChoiceIconImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if(assetPath!="Assets/Resources/UI/Generated20261018/rune_stats.png") return;
            var importer=(TextureImporter)assetImporter;
            importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;
            importer.mipmapEnabled=false;importer.filterMode=FilterMode.Point;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;importer.maxTextureSize=2048;
        }
    }
}
