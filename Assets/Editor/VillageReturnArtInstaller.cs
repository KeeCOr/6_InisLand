using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class VillageReturnArtInstaller
    {
        public static void Install()
        {
            AssetDatabase.Refresh();
            var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/UI/StoryIllustrations/village_return.png");
            importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=false;
            importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;
            importer.npotScale=TextureImporterNPOTScale.None;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;
            importer.SaveAndReimport();AssetDatabase.SaveAssets();
        }
    }
}
