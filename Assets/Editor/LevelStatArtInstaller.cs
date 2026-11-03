using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class LevelStatArtInstaller
    {
        public static void Install()
        {
            AssetDatabase.Refresh();
            foreach(string key in new[]{"damage-up","fire-rate-up","hp-up","range-up","move-speed-up","melee-damage-up","ranged-damage-up","scope-melee","scope-ranged","scope-common"})
            {
                string name=key.StartsWith("scope-")?"rune-"+key:"rune-choice-"+key;
                string path="Assets/Resources/UI/rune_choices/"+name+".png";
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;
                importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;
                importer.wrapMode=TextureWrapMode.Clamp;importer.npotScale=TextureImporterNPOTScale.None;
                importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=256;
                importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();Debug.Log("Level stat art imported:7 upgrade icons and3 scope badges.");
        }
    }
}
