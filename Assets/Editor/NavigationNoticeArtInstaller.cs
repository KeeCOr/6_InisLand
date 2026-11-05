using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class NavigationNoticeArtInstaller
    {
        public static void Install()
        {
            AssetDatabase.Refresh();
            foreach(var key in new[]{"arrow","home","damage"})
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/"+NavigationIconSkin.Folder+key+".png");
                importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
                importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;importer.npotScale=TextureImporterNPOTScale.None;
                importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=256;importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
        }
    }
}
