using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class RuneFrameArtInstaller
    {
        public static void Install()
        {
            AssetDatabase.Refresh();
            foreach(string name in new[]{"frame","card"})
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/UI/LevelUp20261104/"+name+".png");
                importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
                importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;importer.npotScale=TextureImporterNPOTScale.None;
                importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
        }
    }
}
