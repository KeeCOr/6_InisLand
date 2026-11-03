using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class SnowfallResourceInstaller
    {
        public static void Install()
        {
            AssetDatabase.Refresh();
            foreach(string key in new[]{"flake","bootprint"})
            {
                string path="Assets/Resources/Sprites/FX/Snowfall20261103/"+key+".png";
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;
                importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;
                importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.maxTextureSize=256;importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();Debug.Log("Snowfall and daytime bootprint textures installed.");
        }
    }
}
