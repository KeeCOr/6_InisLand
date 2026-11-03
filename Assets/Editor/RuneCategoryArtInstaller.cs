using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class RuneCategoryArtInstaller
    {
        public static void Install()
        {
            AssetDatabase.Refresh();
            var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/UI/rune_choices/rune-choice-melee-cleave.png");
            importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
            importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;
            importer.npotScale=TextureImporterNPOTScale.None;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=256;
            importer.SaveAndReimport();AssetDatabase.SaveAssets();
        }
    }
}
