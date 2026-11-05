using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class HomeNavigationArtInstaller
    {
        public static void Install()
        {
            AssetDatabase.Refresh();var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/"+HomeNavigation.IconPath+".png");
            importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
            importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;importer.npotScale=TextureImporterNPOTScale.None;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=128;importer.SaveAndReimport();AssetDatabase.SaveAssets();
        }
    }
}
