using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class QuestFrameArtInstaller
    {
        public static void Install()
        {
            AssetDatabase.Refresh();
            foreach(var path in new[]{QuestTrackerFrameSkin.FramePath,QuestTrackerFrameSkin.RibbonPath})
            {
            var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/"+path+".png");
            importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
            importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;importer.npotScale=TextureImporterNPOTScale.None;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
        }
    }
}
