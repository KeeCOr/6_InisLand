using System.IO;
using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class RockVariantInstaller
    {
        [System.Serializable] private class Record{public string runtime;public float ppu;public float pivot_y;}
        [System.Serializable] private class Manifest{public Record[] assets;}
        public static void Install()
        {
            AssetDatabase.Refresh();
            var baseline=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Sprites/Props/snowRock.png");
            var baselineSettings=new TextureImporterSettings();baseline.ReadTextureSettings(baselineSettings);
            baselineSettings.spriteMeshType=SpriteMeshType.Tight;baseline.SetTextureSettings(baselineSettings);baseline.SaveAndReimport();
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText("Assets/art/generated/environment/rocks20261103/generation_manifest.json"));
            var additional=JsonUtility.FromJson<Manifest>(File.ReadAllText("Assets/art/generated/environment/rocks20261105/generation_manifest.json"));
            var assets=new System.Collections.Generic.List<Record>(manifest.assets);assets.AddRange(additional.assets);
            foreach(var asset in assets)
            {
                var imp=(TextureImporter)AssetImporter.GetAtPath(asset.runtime);
                imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.spritePixelsPerUnit=asset.ppu;
                var settings=new TextureImporterSettings();imp.ReadTextureSettings(settings);
                settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=new Vector2(.5f,asset.pivot_y);settings.spriteMeshType=SpriteMeshType.Tight;
                imp.SetTextureSettings(settings);imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.filterMode=FilterMode.Bilinear;
                imp.npotScale=TextureImporterNPOTScale.None;imp.maxTextureSize=4096;imp.wrapMode=TextureWrapMode.Clamp;
                imp.textureCompression=TextureImporterCompression.Uncompressed;imp.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
        }
    }
}
