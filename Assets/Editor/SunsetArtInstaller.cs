using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class SunsetArtInstaller
    {
        public static void Install()
        {
            AssetDatabase.Refresh();
            var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/UI/Generated20261103/sunset_warning.png");
            importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;
            importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=256;importer.SaveAndReimport();
            AssetDatabase.SaveAssets();
        }
    }
}
